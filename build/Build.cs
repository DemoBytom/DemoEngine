using System.Collections.Generic;
using Nuke.Common;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

[CheckBuildProjectConfigurations]
[UnsetVisualStudioEnvironmentVariables]
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
    private IReadOnlyCollection<AbsolutePath> TestProjects => TestDirectory.GlobFiles("**/*.csproj");

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
        .OnlyWhenDynamic(() => TestProjects.Count > 0)
        .Executes(() =>
        {
            DotNetTest(_ => _
                .SetConfiguration(_configuration)
                .EnableNoRestore()
                .EnableNoBuild()
                .CombineWith(TestProjects, (oo, testProj) => oo
                    .SetProjectFile(testProj)),
                degreeOfParallelism: TestProjects.Count,
                completeOnFailure: true);
        });

    private Target Publish => _ => _
        .DependsOn(Test)
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
        });

    private Target Full => _ => _.DependsOn(Clean, Compile, Test, Publish);
}