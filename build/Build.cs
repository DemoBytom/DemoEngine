// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Diagnostics;
using System.IO.Compression;
using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.CoverallsNet;
using Nuke.Common.Tools.Coverlet;
using Nuke.Common.Tools.DotCover;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Tools.ReportGenerator;
using Nuke.Common.Utilities;
using Nuke.Common.Utilities.Collections;
using Serilog;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.CoverallsNet.CoverallsNetTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.Git.GitTasks;
using static Nuke.Common.Tools.ReportGenerator.ReportGeneratorTasks;

namespace BuildScript;

[UnsetVisualStudioEnvironmentVariables]
[GitHubActions(
    "CI",
    GitHubActionsImage.WindowsLatest,
    On =
    [
        GitHubActionsTrigger.Push
    ],
    InvokedTargets =
    [
        nameof(Clean),
        nameof(Compile),
        nameof(Test),
        nameof(Publish)
    ],
    EnableGitHubToken = true,
    ImportSecrets =
    [
        nameof(CoverallsToken)
    ],
    FetchDepth = 0,
    Lfs = true)]
internal partial class Build : NukeBuild
{
    /* Install Global Tool
     * - $ dotnet tool install Nuke.GlobalTool --global
     *
     * To run the build using Global Tool
     * - $ nuke Full
     *
     * To run the build using powershell, without Global Tool
     * - PS> .\build.ps1 Full
     *
     * To run the build using shell, without Global Tool
     * - $ ./build.sh Full
     *
     * Support plugins are available for:
     * - JetBrains ReSharper https://nuke.build/resharper
     * - JetBrains Rider https://nuke.build/rider
     * - Microsoft VisualStudio https://nuke.build/visualstudio
     * - Microsoft VSCode https://nuke.build/vscode
     * */

    public static int Main() => Execute<Build>(x => x.Publish);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    public readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    //[Parameter("Self contained application rids")]
    //public readonly string[] RIDs = [];

    [Parameter("Coveralls token")]
    public readonly string? CoverallsToken = null;

    [Parameter("Coveralls jobId")]
    public readonly string? CoverallsJobID = EnvironmentInfo.Variables switch
    {
        var gh when gh.TryGetValue("GITHUB_RUN_ID", out var ghid) => ghid,
        var nuke when nuke.TryGetValue("NUKE_RUN_ID", out var nukeid) => nukeid,
        _ => null
    };

    [Solution] public readonly Solution Solution = default!;
    [GitRepository] private readonly GitRepository _gitRepository = default!;

    private GitVersion _gitVersion = default!;

    private static AbsolutePath SourceDirectory => RootDirectory / "src";
    private static AbsolutePath TestDirectory => RootDirectory / "test";
    private static AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";
    private Project[] TestProjects => Solution.GetAllProjects("*.UTs").ToArray();

    //private const string MASTER_BRANCH = "master";
    private const string DEVELOP_BRANCH = "develop";

    //private const string RELEASE_BRANCH_PREFIX = "release";
    //private const string HOTFIX_BRANCH_PREFIX = "hotfix";

    protected override void OnBuildInitialized()
    {
        base.OnBuildInitialized();
        var resp = Git("rev-parse --is-shallow-repository");
        if (bool.TryParse(resp.FirstOrDefault().Text, out var isShallow) && isShallow)
        {
            Log.Information("Unshallowing the repository");
            _ = Git("fetch origin +refs/heads/*:refs/remotes/origin/* --unshallow --quiet");
        }

        _ = Git("fetch --all --tags --quiet");
        Environment.SetEnvironmentVariable("IGNORE_NORMALISATION_GIT_HEAD_MOVE", "1");
        _gitVersion = GitVersionTasks
            .GitVersion(s => s
                .SetNoFetch(false)
                .SetNoCache(true)
                .DisableProcessOutputLogging()
                )
            .Result;
    }

    public Target Clean => t => t
        .Before(Restore, Compile, Test, Publish)
        .Executes(() =>
        {
            if (!Debugger.IsAttached)
            {
                SourceDirectory
                    .GlobDirectories("**/bin", "**/obj")
                    .ForEach(path
                        => path.DeleteDirectory());
                TestDirectory
                    .GlobDirectories("**/bin", "**/obj")
                    .ForEach(path
                        => path.DeleteDirectory());
            }
            _ = ArtifactsDirectory.CreateOrCleanDirectory();
        });

    public Target Restore => _ => _
        .Executes(() => DotNetRestore(_ => _
                .SetProjectFile(Solution)));

#pragma warning disable CA1822 // Mark members as static

    /* Top level statements in program.cs still cause issues with dotnet format
     * https://github.com/dotnet/format/issues/1567 */

    public Target VerifyCodeFormat => _ => _
        .Executes(() => DotNet(
            /* `--exclude **\Program.cs` 
             * to work around the fact that it still doesn't handle top level statements properly */
            @"format -v n --verify-no-changes --exclude **\Program.cs"));

#pragma warning restore CA1822 // Mark members as static

    public Target Compile => _ => _
        .DependsOn(Restore, VerifyCodeFormat)
        .Executes(() => DotNetBuild(_ => _
            .SetProjectFile(Solution)
            .SetNoRestore(InvokedTargets.Contains(Restore))
            .SetConfiguration(Configuration)
            //.SetProperty("Platform", "x64")
            .SetAssemblyVersion(_gitVersion.AssemblySemVer)
            .SetFileVersion(_gitVersion.AssemblySemFileVer)
            .SetInformationalVersion(_gitVersion.InformationalVersion)
            .EnableNoRestore()));

    public Target Test => _ => _
        .DependsOn(Compile)
        .OnlyWhenDynamic(() => TestProjects.Length > 0)
        //.Produces(
        //    ArtifactsDirectory / "*.trx",
        //    ArtifactsDirectory / "*.xml")
        .Executes(() => DotNetTest(_ => _
            .SetNoRestore(InvokedTargets.Contains(Restore))
            .SetNoBuild(InvokedTargets.Contains(Compile))
            .SetConfiguration(Configuration)
            //.SetProperty("Platform", "x64")
            .SetProperty("CollectCoverage", true)
            .SetProperty("CoverletOutputFormat", "opencover")
            //.SetProperty("ExcludeByFile", "*.Generated.cs")
            .SetResultsDirectory(ArtifactsDirectory)
            .CombineWith(TestProjects, (oo, testProj) => oo
                .SetProjectFile(testProj)
                .AddLoggers($"trx;LogFileName={testProj.Name}.trx")
                .SetProperty("CoverletOutput", ArtifactsDirectory / $"{testProj.Name}.xml")),
            /* For now the parallel execution seems to be broken due to new code coverage collection
             * when running dotnet test.
             * dotnet test seems to be trying to restore projects that are currently being tested in parallel,
             * In future consider moving to dotnet test *.sln and let that handle parallelism 
             */
            degreeOfParallelism: 1,//TestProjects.Length,
            completeOnFailure: true));

    public Target Coverage => t => t
        .TriggeredBy(Test)
        .DependsOn(Test)
        .Produces(ArtifactsDirectory / "coverage.zip")
        .Executes(() =>
        {
            _ = ReportGenerator(_ => _
                .SetReports(ArtifactsDirectory / "*.xml")
                .SetReportTypes(ReportTypes.HtmlInline)
                .SetTargetDirectory(ArtifactsDirectory / "coverage"));

            if (ScheduledTargets.Contains(UploadCoveralls))
            {
                _ = ReportGenerator(_ => _
                    .SetReports(ArtifactsDirectory / "*.xml")
                    .SetReportTypes(ReportTypes.Xml)
                    .SetTargetDirectory(ArtifactsDirectory / "coveralls"));
            }

            (ArtifactsDirectory / "coverage").ZipTo(
                archiveFile: ArtifactsDirectory / "coverage.zip",
                fileMode: FileMode.Create);
        });

    public Target Publish => _ => _
        .DependsOn(Compile, Test)
        .After(Test)
        .Produces(
            ArtifactsDirectory / "Demo.Engine.zip",
            ArtifactsDirectory / "Demo.Engine.win-x64.zip",
            ArtifactsDirectory / "Demo.Engine.win-arm64.zip")
        .Executes(() =>
        {
            //A runtime dependant version is also currently generated by default and exposed to CI artifacts
            PublishApp(
                projectName: "Demo.Engine");

            //We generate a self contained, "one file", trimmed version for Windows x64 by default
            //Any other can be generated as well, but aren't currently supported so aren't exposed to CI artifacts
            ReadOnlySpan<string> rids =
            [
                "win-x64",
                "win-arm64",
            ];

            foreach (var rid in rids)
            {
                PublishApp(
                    projectName: "Demo.Engine",
                    rid: rid);
            }
        });

    public Target UploadCoveralls => t => t
        .TriggeredBy(Test)
        .DependsOn(Test, Coverage)
        .OnlyWhenStatic(() => IsServerBuild || Debugger.IsAttached)
        .OnlyWhenDynamic(() => GitHasCleanWorkingCopy())
        .Requires(() => CoverallsToken)
        .Requires(() => CoverallsJobID)
        .Executes(() =>
        {
            var gitShow = Git("show -s --format=%H%n%cN%n%ce%n%B");
            Assert.True(gitShow.Count >= 4, "wrong GIT show return!");

            var commitID = gitShow.ElementAt(0).Text;
            var authorName = gitShow.ElementAt(1).Text;
            var authorMail = gitShow.ElementAt(2).Text;

            var commitBody = string
                .Join(
                    Environment.NewLine,
                    gitShow
                        .ToArray()[3..]
                        .Select(o => o.Text))
                .Trim();
            Environment.SetEnvironmentVariable("DOTNET_ROLL_FORWARD", "LatestMajor");

            _ = CoverallsNet(toolSettings => toolSettings
                .SetDryRun(Debugger.IsAttached)
                .SetRepoToken(CoverallsToken)
                .SetUserRelativePaths(true)
                .SetCommitBranch(_gitRepository.Branch)
                .SetCommitId(commitID)
                .SetCommitAuthor(authorName)
                .SetCommitEmail(authorMail)
                .SetCommitMessage(commitBody)
                .SetInput(ArtifactsDirectory / "coveralls")
                .SetJobId(CoverallsJobID)
                .SetProcessAdditionalArguments(
                    "--reportgenerator")
                );
        });

    private void PublishApp(string projectName, string? rid = null)
    {
        AbsolutePath outputDir;
        if (string.IsNullOrEmpty(rid))
        {
            outputDir = ArtifactsDirectory / projectName;
            Log.Information($"Publishing {projectName} into {outputDir}");
        }
        else
        {
            outputDir = ArtifactsDirectory / $"{projectName}.{rid}";
            Log.Information($"Publishing {projectName} for {rid} into {outputDir}");
        }

        _ = DotNetPublish(t => t
            .SetProject(
                /* This doesn't work, because Demo.Engine is in a solution folder
                 * And Solution.GetProject only searches through projects that are directly in the Solution
                 * */
                //Solution.GetProject(projectName))
                v: Solution
                    .AllProjects
                    .Single(project
                        => project.Name.Equals(projectName, StringComparison.Ordinal)))
            .SetConfiguration(Configuration)
            //.SetProperty("Platform", platform)
            .SetAssemblyVersion(_gitVersion.AssemblySemVer)
            .SetFileVersion(_gitVersion.AssemblySemFileVer)
            .SetInformationalVersion(_gitVersion.InformationalVersion)
            .SetOutput(outputDir)
            .When(string.IsNullOrEmpty(rid), _ => _
                .SetNoRestore(InvokedTargets.Contains(Restore))
                .SetNoBuild(InvokedTargets.Contains(Compile)))
            .When(!string.IsNullOrEmpty(rid), _ => _
                .SetNoRestore(false)
                .SetNoBuild(false)
                .SetSelfContained(true)
                .EnablePublishSingleFile()
                //.SetProperty("PublishSingleFile", true)
                /*Trimming is unsupported by windows-forms and has been disabled for .NET 6.0!
                    * https://github.com/dotnet/runtime/issues/58894
                    * https://docs.microsoft.com/en-us/dotnet/core/deploying/trimming/incompatibilities
                    * https://github.com/dotnet/winforms/issues/4649
                    * */
                .DisablePublishTrimmed()
                //.SetProperty("PublishTrimmed", false)
                .SetRuntime(rid)
                .SetProperty("PublishReadyToRun", true)
                ))
            ;

        outputDir.ZipTo(
            archiveFile: $"{outputDir}.zip",
            compressionLevel: CompressionLevel.Optimal,
            fileMode: FileMode.Create);
    }

    public Target Full => _ => _
        .DependsOn(
            Clean,
            Compile,
            Test,
            Publish)
        .Unlisted();
}