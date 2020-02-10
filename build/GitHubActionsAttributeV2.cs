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
                if (baseJob.Steps[i] is GitHubActionsUsingStep a &&
                    a.Using.Equals(
                        "actions/checkout@v1",
                        StringComparison.OrdinalIgnoreCase))
                {
                    baseJob.Steps[i] = new GitHubActionsUsingWithStep
                    {
                        Using = "actions/checkout@v2",
                        Withs = new[]
                        {
                            ("fetch-depth", "0"),
                            ("lfs", "true")
                        }
                    };
                }
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
}