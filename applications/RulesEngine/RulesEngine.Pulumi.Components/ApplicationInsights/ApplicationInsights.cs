using Pulumi;
using Pulumi.AzureNative.Insights.V20200202;
using RulesEngine.Pulumi.Components.Helpers;
using RulesEngine.Pulumi.Components.Tags;

namespace RulesEngine.Pulumi.Components.ApplicationInsights
{
    public class ApplicationInsights: ComponentResource
    {
        public Output<string> InstrumentationKey { get; set; }
        public Output<string> ConnectionString { get; set; }


        public ApplicationInsights(ApplicationInsightsArgs args) : base(
            $"{StringConstants.ComponentNamespace}:component:ApplicationInsights",
            args.Name)
        {
            var appInsights = new Component($"{args.Name}-ai", new ComponentArgs
            {
                ApplicationType = "web",
                Kind = "web",
                ResourceGroupName = args.ResourceGroup,
                Tags = args.Tags,
                WorkspaceResourceId = args.WorkSpaceId,
            }, new CustomResourceOptions {Parent = this, IgnoreChanges = TagConstants.TagChangesToIgnore.ToList(), Protect = true});

            InstrumentationKey = appInsights.InstrumentationKey;
            ConnectionString = appInsights.ConnectionString;
        }
    }
}
