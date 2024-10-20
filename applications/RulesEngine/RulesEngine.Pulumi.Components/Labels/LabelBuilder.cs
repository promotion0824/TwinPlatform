using Pulumi;
using RulesEngine.Pulumi.Components.Tags;

namespace RulesEngine.Pulumi.Components.Labels
{
    public class LabelBuilder
    {

        private readonly InputMap<string> _labels;

        private string? Environment { get; }

        public LabelBuilder(string app, string customer, string environment)
        {
            Environment = environment;
            _labels = new()
            {
                {"stack", System.Environment.GetEnvironmentVariable("STACK_NAME") ?? Deployment.Instance.StackName},
                {"company", "willow"},
                {"customer", customer},
                {"app", app},
                {"managedby", "pulumi"},
                {"team", "ruleEngine"},
                {"environment", Environment ?? System.Environment.GetEnvironmentVariable("ENVIRONMENT_NAME") ?? "UNKOWN"},
                {"project", System.Environment.GetEnvironmentVariable("DEFINITION_LOCATION") ?? System.Environment.GetEnvironmentVariable("BUILD_REPOSITORY_URI") ?? "willowdev/AzurePlatform"},
                {"created", $"{DateTimeOffset.UtcNow}"},
                {"PipelineName", System.Environment.GetEnvironmentVariable("DEFINITION_NAME") ??System.Environment.GetEnvironmentVariable("BUILD_DEFINITIONNAME") ?? System.Environment.GetEnvironmentVariable("RELEASE_DEFINITIONNAME") ?? "UNKOWN"},
                {"PipelineId", System.Environment.GetEnvironmentVariable("DEFINITION_ID") ??System.Environment.GetEnvironmentVariable("BUILD_BUILDNUMBER") ?? System.Environment.GetEnvironmentVariable("RELEASE_RELEASENAME") ?? "UNKOWN"}
            };
        }

        LabelBuilder WithExtraTag(string tag, string value)
        {
	        _labels.Add(tag, value);
            return this;
        }

        public InputMap<string> Build()
        {
            return _labels;
        }
    }
}
