// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Nuke.Common;
using Nuke.Common.Git;
using Serilog;
using static Nuke.Common.ChangeLog.ChangelogTasks;
using static Nuke.Common.Tools.Git.GitTasks;

namespace BuildScript;

internal partial class Build
{
    private readonly string _changelogFile = RootDirectory / "CHANGELOG.MD";

    public Target Changelog => _ => _
        .OnlyWhenStatic(() =>
            _gitRepository.IsOnReleaseBranch()
            || _gitRepository.IsOnHotfixBranch())
        //.WhenSkipped(DependencyBehavior.Skip)
        .Executes(() =>
        {
            FinalizeChangelog(
                 _changelogFile,
                _gitVersion.MajorMinorPatch,
                _gitRepository);
            //if (false) //TODO one day, maybe
            //{
            //    Git($"add {_changelogFile}");
            //    Git($"commit -m \"Finalize {Path.GetFileName(_changelogFile)} for {_gitVersion.MajorMinorPatch}\"");
            //}
        });

    public Target Release => _ => _
        .DependsOn(Changelog)
        .Requires(() =>
            !_gitRepository.IsOnReleaseBranch()
            || GitHasCleanWorkingCopy())
        .Executes(() =>
        {
            var isRelease = _gitRepository.IsOnReleaseBranch();
            var clean = GitHasCleanWorkingCopy();
            Log.Information($"isRelease {isRelease}, clean {clean}");
        });

    public Target FinishFeature => _ => _
        .DependsOn(Clean, Restore, Compile, Test)
        .Executes(() =>
        {
            var currentBranch = _gitRepository.Branch;
            if (!GitHasCleanWorkingCopy())
            {
                Git("add .");
                Git("commit -m \"automatic commit\"");
            }
            Git($"checkout {DEVELOP_BRANCH}");
            Git($"merge --no-ff --no-edit {currentBranch}");
        });
}