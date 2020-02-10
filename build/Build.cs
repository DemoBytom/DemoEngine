using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using BuildExtensions;
using Nuke.Common;
using Nuke.Common.CI;
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
using Nuke.Common.Tools.InspectCode;
using Nuke.Common.Tools.ReportGenerator;
using Nuke.Common.Utilities;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.ControlFlow;
using static Nuke.Common.IO.CompressionTasks;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.CoverallsNet.CoverallsNetTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.Git.GitTasks;
using static Nuke.Common.Tools.ReportGenerator.ReportGeneratorTasks;

namespace BuildScript
{
    [CheckBuildProjectConfigurations]
    [UnsetVisualStudioEnvironmentVariables]
    [GitHubActionsV2(
        "CI",
        GitHubActionsImage.WindowsLatest,
        On = new[]
        {
            GitHubActionsTrigger.Push
        },
        InvokedTargets = new[]
        {
            nameof(Clean),
            nameof(Compile),
            nameof(Test),
            nameof(Publish)
        },
        ImportGitHubTokenAs = nameof(GitHubToken),
        ImportSecrets = new[]
        {
            nameof(CoverallsToken)
        })]
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
        public readonly Configuration Config = IsLocalBuild ? Configuration.Debug : Configuration.Release;

        [Parameter("GitHub token")]
        public readonly string GitHubToken = string.Empty;

        [Parameter("Self contained application rids")]
        public readonly string[] RIDs = Array.Empty<string>();

        [Parameter("Coveralls token")]
        public readonly string? CoverallsToken = null;

        [Parameter("Coveralls jobId")]
        public readonly string? CoverallsJobID = EnvironmentInfo.Variables switch
        {
            var gh when gh.TryGetValue("GITHUB_RUN_ID", out var ghid) => ghid,
            var nuke when nuke.TryGetValue("NUKE_RUN_ID", out var nukeid) => nukeid,
            _ => null
        };

        [Solution] private readonly Solution _solution = default!;
        [GitRepository] private readonly GitRepository _gitRepository = default!;

        //[GitVersion] private readonly GitVersion _gitVersion = default!;
        private GitVersion _gitVersion = default!;

        private AbsolutePath SourceDirectory => RootDirectory / "src";
        private AbsolutePath TestDirectory => RootDirectory / "test";
        private AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";
        private Project[] TestProjects => _solution.GetProjects("*.UTs").ToArray();

        private const string MASTER_BRANCH = "master";
        private const string DEVELOP_BRANCH = "develop";
        private const string RELEASE_BRANCH_PREFIX = "release";
        private const string HOTFIX_BRANCH_PREFIX = "hotfix";

        protected override void OnBuildInitialized()
        {
            base.OnBuildInitialized();
            var resp = Git("rev-parse --is-shallow-repository");
            if (bool.TryParse(resp.FirstOrDefault().Text, out var isShallow) && isShallow)
            {
                Logger.Info("Unshallowing the repository");
                Git("fetch origin +refs/heads/*:refs/remotes/origin/* --unshallow --quiet");
            }

            _gitVersion = GitVersionTasks
                .GitVersion(s => s
                    .SetNoFetch(true)
                    .SetNoCache(true)
                    .SetVerbosity(GitVersionVerbosity.debug)
                    .SetFramework("netcoreapp3.1")
                    .DisableLogOutput())
                .Result;
        }

        private Target Clean => _ => _
            .Before(Restore, Compile, Test, Publish)
            .Executes(() =>
            {
                if (!Debugger.IsAttached)
                {
                    SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
                    TestDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
                }
                EnsureCleanDirectory(ArtifactsDirectory);
            });

        private Target Restore => _ => _
            .Executes(() =>
            {
                DotNetRestore(_ => _
                    .SetProjectFile(_solution));
            });

        private Target Compile => _ => _
            .DependsOn(Restore)
            .Executes(() =>
            {
                DotNetBuild(_ => _
                    .SetProjectFile(_solution)
                    .SetNoRestore(ExecutingTargets.Contains(Restore))
                    .SetConfiguration(Config)
                    .SetAssemblyVersion(_gitVersion.AssemblySemVer)
                    .SetFileVersion(_gitVersion.AssemblySemFileVer)
                    .SetInformationalVersion(_gitVersion.InformationalVersion)
                    .EnableNoRestore());
            });

        private Target Test => _ => _
            .DependsOn(Compile)
            .OnlyWhenDynamic(() => TestProjects.Length > 0)
            //.Produces(
            //    ArtifactsDirectory / "*.trx",
            //    ArtifactsDirectory / "*.xml")
            .Executes(() =>
            {
                DotNetTest(_ => _
                    .SetConfiguration(Config)
                        .SetNoRestore(ExecutingTargets.Contains(Restore))
                        .SetNoBuild(ExecutingTargets.Contains(Compile))
                        .SetProperty("CollectCoverage", propertyValue: true)
                        .SetProperty("CoverletOutputFormat", "opencover")
                    //.SetProperty("ExcludeByFile", "*.Generated.cs")
                    .SetResultsDirectory(ArtifactsDirectory)
                    .CombineWith(TestProjects, (oo, testProj) => oo
                        .SetProjectFile(testProj)
                        .SetLogger($"trx;LogFileName={testProj.Name}.trx")
                        //.SetLogger($"xunit;LogFileName={testProj.Name}.xml")
                        .SetProperty("CoverletOutput", ArtifactsDirectory / $"{testProj.Name}.xml")),
                    degreeOfParallelism: TestProjects.Length,
                    completeOnFailure: true);
            });

        private Target Coverage => _ => _
            .TriggeredBy(Test)
            .DependsOn(Test)
            .Produces(ArtifactsDirectory / "coverage.zip")
            .Executes(() =>
            {
                ReportGenerator(_ => _
                    .SetFramework("netcoreapp3.0")
                    .SetReports(ArtifactsDirectory / "*.xml")
                    .SetReportTypes(ReportTypes.HtmlInline)
                    .SetTargetDirectory(ArtifactsDirectory / "coverage"));

                if (ExecutingTargets.Contains(UploadCoveralls))
                {
                    ReportGenerator(_ => _
                        .SetFramework("netcoreapp3.0")
                        .SetReports(ArtifactsDirectory / "*.xml")
                        .SetReportTypes(ReportTypes.Xml)
                        .SetTargetDirectory(ArtifactsDirectory / "coveralls"));
                }

                CompressZip(
                    directory: ArtifactsDirectory / "coverage",
                    archiveFile: ArtifactsDirectory / "coverage.zip",
                    fileMode: FileMode.Create);
            });

        private Target Publish => _ => _
            .DependsOn(Compile, Test)
            .After(Test)
            .Produces(
                ArtifactsDirectory / "Demo.Engine.zip",
                ArtifactsDirectory / "Demo.Engine.win10-x64.zip")
            .Executes(() =>
            {
                //A runtime dependant version is also currently generated by default and exposed to CI artifacts
                PublishApp("Demo.Engine");

                //We generate a self contained, "one file", trimmed version for Windows 10 x64 by default
                //Any other can be generated as well, but aren't currently supported so aren't exposed to CI artifacts
                foreach (var rid in RIDs.Concat("win10-x64"))
                {
                    PublishApp("Demo.Engine", rid);
                }
            });

        private Target UploadCoveralls => _ => _
            .TriggeredBy(Test)
            .DependsOn(Test, Coverage)
            .OnlyWhenStatic(() => IsServerBuild || Debugger.IsAttached)
            .OnlyWhenDynamic(() => GitHasCleanWorkingCopy())
            .Requires(() => CoverallsToken)
            .Requires(() => CoverallsJobID)
            .Executes(() =>
            {
                var gitShow = Git("show -s --format=%H%n%cN%n%ce%n%B");
                Assert(gitShow.Count >= 4, "wrong GIT show return!");

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

                CoverallsNet(toolSettings => toolSettings
                    .SetDryRun(Debugger.IsAttached)
                    .SetRepoToken(CoverallsToken)
                    .SetUserRelativePaths(true)
                    .SetCommitBranch(_gitRepository.Branch)
                    .SetCommitId(commitID)
                    .SetCommitAuthor(authorName)
                    .SetCommitEmail(authorMail)
                    .SetCommitMessage(commitBody)
                    .SetInput(ArtifactsDirectory / "coveralls")
                    .SetArgumentConfigurator(argumentConfigurator =>
                        argumentConfigurator
                            .Add("--jobId")
                            .Add(CoverallsJobID)
                            .Add("--reportgenerator"))
                    );
            });

        private void PublishApp(string projectName, string? rid = null)
        {
            AbsolutePath outputDir;
            if (string.IsNullOrEmpty(rid))
            {
                outputDir = ArtifactsDirectory / projectName;
                Logger.Info($"Publishing {projectName} into {outputDir}");
            }
            else
            {
                outputDir = ArtifactsDirectory / $"{projectName}.{rid}";
                Logger.Info($"Publishing {projectName} for {rid} into {outputDir}");
            }

            DotNetPublish(_ => _
                        .SetProject(_solution.GetProject(projectName))
                        .SetConfiguration(Config)
                        .SetAssemblyVersion(_gitVersion.AssemblySemVer)
                        .SetFileVersion(_gitVersion.AssemblySemFileVer)
                        .SetInformationalVersion(_gitVersion.InformationalVersion)
                        .SetOutput(outputDir)
                        .When(string.IsNullOrEmpty(rid), _ => _
                            .SetNoRestore(ExecutingTargets.Contains(Restore))
                            .SetNoBuild(ExecutingTargets.Contains(Compile)))
                        .When(!string.IsNullOrEmpty(rid), _ => _
                            .SetNoRestore(false)
                            .SetNoBuild(false)
                            .SetSelfContained(true)
                            .SetProperty("PublishSingleFile", true)
                            .SetProperty("PublishTrimmed", true)
                            .SetRuntime(rid)));

            CompressZip(
                directory: outputDir,
                archiveFile: $"{outputDir}.zip",
                compressionLevel: CompressionLevel.Optimal,
                fileMode: FileMode.Create);
        }

        private Target Full => _ => _.DependsOn(Clean, Compile, Test, Publish).Unlisted();
    }
}