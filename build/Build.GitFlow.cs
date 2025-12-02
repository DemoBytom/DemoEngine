// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Nuke.Common;
using static Nuke.Common.ChangeLog.ChangelogTasks;

namespace BuildScript;

internal partial class Build
{
    private const string DEVELOP_BRANCH = "develop";

    private readonly string _changelogFile = RootDirectory / "CHANGELOG.MD";

    public Target Changelog => _ => _
        //.OnlyWhenStatic(() =>
        //    _gitRepository.IsOnReleaseBranch()
        //    || _gitRepository.IsOnHotfixBranch())
        //.WhenSkipped(DependencyBehavior.Skip)
        .Executes(() =>
        {
            FinalizeChangelog(
                 _changelogFile,
                _gitVersioning.SemVer2,
                _gitRepository);
            //if (false) //TODO one day, maybe
            //{
            //    Git($"add {_changelogFile}");
            //    Git($"commit -m \"Finalize {Path.GetFileName(_changelogFile)} for {_gitVersion.MajorMinorPatch}\"");
            //}
        });
}