// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.Execution;
using Nuke.Common.Utilities;

namespace BuildExtensions
{
    public class GitHubActionsV2Attribute : GitHubActionsAttribute
    {
        public GitHubActionsV2Attribute(string name, GitHubActionsImage image, params GitHubActionsImage[] images)
            : base(name, image, images)
        {
        }

        protected override GitHubActionsJob GetJobs(GitHubActionsImage image, IReadOnlyCollection<ExecutableTarget> relevantTargets)
        {
            var baseJob = base.GetJobs(image, relevantTargets);

            for (var i = 0; i < baseJob.Steps.Length; ++i)
            {
                baseJob.Steps[i] = baseJob.Steps[i] switch
                {
                    GitHubActionsUsingStep { Using: "actions/checkout@v1" }
                        => new GitHubActionsUsingWithStep
                        {
                            Using = "actions/checkout@v2",
                            Withs = new[]
                            {
                                ("fetch-depth", "0"),
                                ("lfs", "true")
                            }
                        },
                    GitHubActionsArtifactStep artifactStep
                        => GitHubActionsArtifactV2Step.FromBase(artifactStep),
                    var @default
                        => @default
                };
            }

            return baseJob;
        }
    }

    public class GitHubActionsUsingWithStep : GitHubActionsUsingStep
    {
        public IEnumerable<(string @using, string @value)> Withs { get; set; } = Array.Empty<(string, string)>();

        public override void Write(CustomFileWriter writer)
        {
            base.Write(writer);
            if (Withs.Any())
            {
                using (writer.Indent())
                {
                    writer.WriteLine("with:");

                    using (writer.Indent())
                    {
                        foreach (var (@using, @value) in Withs)
                        {
                            writer.WriteLine($"{@using}: {@value}");
                        }
                    }
                }
            }
        }
    }

    public class GitHubActionsArtifactV2Step : GitHubActionsArtifactStep
    {
        public override void Write(CustomFileWriter writer)
        {
            writer.WriteLine("- uses: actions/upload-artifact@v2");

            using (writer.Indent())
            {
                writer.WriteLine("with:");
                using (writer.Indent())
                {
                    writer.WriteLine($"name: {Name}");
                    writer.WriteLine($"path: {Path}");
                }
            }
        }

        public static GitHubActionsArtifactV2Step FromBase(GitHubActionsArtifactStep step) =>
            new()
            {
                Name = step.Name,
                Path = step.Path
            };
    }
}