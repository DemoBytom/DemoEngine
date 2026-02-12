// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Diagnostics;
using System.IO.Compression;
using System.IO.Pipelines;
using System.Text.Json;
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
using Nuke.Common.Tools.NerdbankGitVersioning;
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
internal sealed partial class Build : NukeBuild
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

    [Parameter("Coveralls token")]
    public readonly string? CoverallsToken = null;

    [Parameter("Coveralls jobId")]
    public readonly string? CoverallsJobID = EnvironmentInfo.Variables switch
    {
        var gh when gh.TryGetValue("GITHUB_RUN_ID", out var ghid) => ghid,
        var nuke when nuke.TryGetValue("NUKE_RUN_ID", out var nukeid) => nukeid,
        _ => null
    };

    [Solution(GenerateProjects = true)] public readonly Solution Solution = default!;
    [GitRepository] private readonly GitRepository _gitRepository = default!;

    [NerdbankGitVersioning]
    private NerdbankGitVersioning _gitVersioning = default!;

    private static AbsolutePath SourceDirectory => RootDirectory / "src";
    private static AbsolutePath TestDirectory => RootDirectory / "test";
    private static AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";
    private Project[] TestProjects => [.. Solution.test.Projects];

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

    public Target Restore => t => t
        .Executes(() => DotNetRestore(t => t
                .SetProjectFile(Solution)));

#pragma warning disable CA1822 // Mark members as static

    /* Top level statements in program.cs still cause issues with dotnet format
     * https://github.com/dotnet/format/issues/1567 */

    public Target VerifyCodeFormat => t => t
        .Executes(() => DotNet(
            /* `--exclude **\Program.cs` 
             * to work around the fact that it still doesn't handle top level statements properly */
            @"format -v n --verify-no-changes --exclude **\Program.cs"));

#pragma warning restore CA1822 // Mark members as static

    public Target Compile => t => t
        .DependsOn(Restore, VerifyCodeFormat)
        .Executes(() => DotNetBuild(t => t
            .SetProjectFile(Solution)
            .SetNoRestore(InvokedTargets.Contains(Restore))
            .SetConfiguration(Configuration)
            .EnableNoRestore()));

    public Target Test => t => t
        .DependsOn(Compile)
        .OnlyWhenDynamic(() => TestProjects.Length > 0)
        //.Produces(
        //    ArtifactsDirectory / "*.trx",
        //    ArtifactsDirectory / "*.xml")
        .Executes(() =>
        {
            ReadOnlySpan<(string os, string arch)> rids =
            [
                ("win", "x64"),
                //("win", "arm64"),
            ];

            foreach (var testProj in TestProjects)
            {
                foreach (var (os, arch) in rids)
                {
                    var coverageOutput = ArtifactsDirectory / $"{testProj.Name}.{os}-{arch}.xml";

                    _ = DotNetRun(run => run
                        .SetProjectFile(testProj)
                        .SetConfiguration("release")
                        //.SetNoRestore(InvokedTargets.Contains(Restore))
                        //.SetNoBuild(InvokedTargets.Contains(Compile))
                        .AddProcessAdditionalArguments(
                            "--arch", arch,
                            "--os", os,
                            "--coverage",
                            "--coverage-output-format", "cobertura",
                            "--disable-logo",
                            "--coverage-output", coverageOutput)
                        );
                }
            }
        });

    public Target Coverage => t => t
        .TriggeredBy(Test)
        .DependsOn(Test)
        .Produces(ArtifactsDirectory / "coverage.zip")
        .Executes(async () =>
        {
            _ = ReportGenerator(t => t
                .SetReports(ArtifactsDirectory / "*.xml")
                .SetReportTypes(ReportTypes.HtmlInline)
                .SetTargetDirectory(ArtifactsDirectory / "coverage"));

            if (GitHubActions.Instance is not null)
            {
                _ = ReportGenerator(t => t
                    .SetReports(ArtifactsDirectory / "*.xml")
                    .SetReportTypes(ReportTypes.MarkdownSummaryGithub)
                    .SetTargetDirectory(ArtifactsDirectory / "coverageGitHub"))
                ;

                using var summary = File.OpenRead(
                    ArtifactsDirectory / "coverageGitHub" / "SummaryGithub.md");
                var pipeReader = PipeReader.Create(summary);

                using var githubSummary = File.Open(
                    EnvironmentInfo.GetVariable("GITHUB_STEP_SUMMARY"),
                    FileMode.Append);
                var pipeWriter = PipeWriter.Create(githubSummary);

                await pipeReader.CopyToAsync(pipeWriter);
            }

            if (ScheduledTargets.Contains(UploadCoveralls))
            {
                _ = ReportGenerator(t => t
                    .SetReports(ArtifactsDirectory / "*.xml")
                    .SetReportTypes(ReportTypes.Xml)
                    .SetTargetDirectory(ArtifactsDirectory / "coveralls"));
            }

            (ArtifactsDirectory / "coverage").ZipTo(
                archiveFile: ArtifactsDirectory / "coverage.zip",
                fileMode: FileMode.Create);
        });

    public Target Publish => t => t
        .DependsOn(Clean, Compile, Test)
        .After(Test)
        .Produces(
            ArtifactsDirectory / "Demo.Engine.zip",
            ArtifactsDirectory / "Demo.Engine.win-x64.zip",
            ArtifactsDirectory / "Demo.Engine.win-arm64.zip")
        .Executes(() =>
        {
            //A runtime dependant version is also currently generated by default and exposed to CI artifacts
            PublishApp(
                Solution.src.Demo_Engine);

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
                    Solution.src.Demo_Engine,
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

    private void PublishApp(Project project, string? rid = null, bool zipProject = true)
    {
        AbsolutePath outputDir;
        if (string.IsNullOrEmpty(rid))
        {
            outputDir = ArtifactsDirectory / project.Name;
            Log.Information($"Publishing {project.Name} into {outputDir}");
        }
        else
        {
            outputDir = ArtifactsDirectory / $"{project.Name}.{rid}";
            Log.Information($"Publishing {project.Name} for {rid} into {outputDir}");
        }

        _ = DotNetPublish(t => t
            .SetProject(
                v: project)
            .SetConfiguration(Configuration)
            .SetOutput(outputDir)
            .When(string.IsNullOrEmpty(rid), t => t
                .SetNoRestore(false)
                .SetNoBuild(false))
            .When(!string.IsNullOrEmpty(rid), t => t
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
        if (zipProject)
        {
            outputDir.ZipTo(
                archiveFile: $"{outputDir}.zip",
                compressionLevel: CompressionLevel.Optimal,
                fileMode: FileMode.Create);
        }
    }

    public Target GitFetch => t => t
        .Executes(() =>
        {
            Log.Information("Current branch {branchName}", GitCurrentBranch());

            _ = Git("fetch --tags --verbose",
                logOutput: true,
                logInvocation: true)
            ;
        });

    public Target PrintVersion => t => t
        .TriggeredBy(Publish)
        .Executes(async () =>
        {
            Log.Information("Generated SemVer {semVer}", _gitVersioning.SemVer2);
            if (GitHubActions.Instance is not null)
            {
                await using var githubSummary = File.Open(
                    EnvironmentInfo.GetVariable("GITHUB_STEP_SUMMARY"),
                    FileMode.Append);
                await using var streamWriter = new StreamWriter(githubSummary);

                await streamWriter.WriteLineAsync();
                await streamWriter.WriteLineAsync(
                    """
                    # Version

                    Generated SemVer:

                    ```json
                    """
                    );

                await streamWriter.FlushAsync();

                await JsonSerializer.SerializeAsync(utf8Json: streamWriter.BaseStream, _gitVersioning, JsonSerializerOptions.Indented);
                await streamWriter.BaseStream.FlushAsync();

                await streamWriter.WriteLineAsync();
                await streamWriter.WriteLineAsync("```");
            }
            else
            {
                Console.WriteLine(JsonSerializer.Serialize(_gitVersioning, JsonSerializerOptions.Indented));
            }
        });

    public Target Full => t => t
        .DependsOn(
            Clean,
            Compile,
            Test,
            Publish)
        .Unlisted();
}

static file class BuildExtensions
{
    private static readonly JsonSerializerOptions _indented = new()
    {
        WriteIndented = true,
    };

    extension(JsonSerializerOptions)
    {
        public static JsonSerializerOptions Indented => _indented;
    }
}