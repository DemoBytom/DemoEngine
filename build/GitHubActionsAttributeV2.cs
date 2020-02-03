using System.Collections.Generic;
using System.Linq;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.Execution;

namespace BuildExtensions
{
    public class GitHubActionsV2Attribute : GitHubActionsAttribute
    {
        public GitHubActionsV2Attribute(string name, GitHubActionsImage image, params GitHubActionsImage[] images) : base(name, image, images)
        {
        }

        protected override GitHubActionsJob GetJobs(GitHubActionsImage image, IReadOnlyCollection<ExecutableTarget> relevantTargets)
        {
            var baseJob = base.GetJobs(image, relevantTargets);

            var job = baseJob
                .Steps
                .Where(s =>
                    s is GitHubActionsUsingStep a
                    && a.Using.Equals(
                        "actions/checkout@v1",
                        System.StringComparison.OrdinalIgnoreCase))
                .Cast<GitHubActionsUsingStep>()
                .FirstOrDefault();
            if (job != null)
            {
                job.Using = "actions/checkout@v2";
            }

            return baseJob;
        }
    }
}