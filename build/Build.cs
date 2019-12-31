using System.IO;
using System.Linq;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.Coverlet;
using Nuke.Common.Tools.DotCover;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Tools.InspectCode;
using Nuke.Common.Tools.ReportGenerator;
using Nuke.Common.Utilities;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.IO.CompressionTasks;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.ReportGenerator.ReportGeneratorTasks;

[CheckBuildProjectConfigurations]
[UnsetVisualStudioEnvironmentVariables]
[GitHubActions(
    "continuous",
    GitHubActionsImage.WindowsLatest)]
internal class Build : NukeBuild
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

    public static int Main() => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    private readonly Configuration _configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Solution] private readonly Solution _solution = default!;
    [GitRepository] private readonly GitRepository _gitRepository = default!;
    [GitVersion] private readonly GitVersion _gitVersion = default!;

    private AbsolutePath SourceDirectory => RootDirectory / "src";
    private AbsolutePath TestDirectory => RootDirectory / "test";
    private AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";
    private Project[] TestProjects => _solution.GetProjects("*.UTs").ToArray();
    //TestDirectory.GlobFiles("**/*.csproj");

    private Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            TestDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
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
               .SetConfiguration(_configuration)
               //.SetAssemblyVersion(_gitVersion.AssemblySemVer)
               //.SetFileVersion(_gitVersion.AssemblySemFileVer)
               //.SetInformationalVersion(_gitVersion.InformationalVersion)
               .SetVersion(_gitVersion.SemVer)
               .EnableNoRestore());
        });

    private Target Test => _ => _
        .DependsOn(Compile)
        .OnlyWhenDynamic(() => TestProjects.Length > 0)
        .Produces(ArtifactsDirectory / "*.trx")
        .Produces(ArtifactsDirectory / "*.xml")
        .Executes(() =>
        {
            DotNetTest(_ => _
                .SetConfiguration(_configuration)
                    .EnableNoRestore()
                    .EnableNoBuild()
                    .SetProperty("CollectCoverage", propertyValue: true)
                    .SetProperty("CoverletOutputFormat", "teamcity%2ccobertura")
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
        .Produces(ArtifactsDirectory / "coverage.zip")
        .Executes(() =>
        {
            ReportGenerator(_ => _
                .SetFramework("netcoreapp3.0")
                .SetReports(ArtifactsDirectory / "*.xml")
                .SetReportTypes(ReportTypes.HtmlInline)
                .SetTargetDirectory(ArtifactsDirectory / "coverage"));

            Logger.Info("Zipping!");
            CompressZip(
                directory: ArtifactsDirectory / "coverage",
                archiveFile: ArtifactsDirectory / "coverage.zip",
                fileMode: FileMode.Create);
        });

    private Target Publish => _ => _
        .DependsOn(Test)
        .Produces(ArtifactsDirectory / "Demo.Engine.zip")
        .Executes(() =>
        {
            DotNetPublish(_ => _
                .SetProject(_solution.GetProject("Demo.Engine"))
                .SetConfiguration(_configuration)
                //.SetAssemblyVersion(_gitVersion.AssemblySemVer)
                //.SetFileVersion(_gitVersion.AssemblySemFileVer)
                //.SetInformationalVersion(_gitVersion.InformationalVersion)
                .SetVersion(_gitVersion.SemVer)
                .EnableNoRestore()
                .EnableNoBuild()
                .SetOutput(ArtifactsDirectory / "Demo.Engine"));

            CompressZip(
                directory: ArtifactsDirectory / "Demo.Engine",
                archiveFile: ArtifactsDirectory / "Demo.Engine.zip",
                fileMode: FileMode.Create);
        });

    private Target Full => _ => _.DependsOn(Clean, Compile, Test, Publish);
}