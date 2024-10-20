using Pulumi;

namespace RulesEngine.Pulumi.Components.ApplicationInsights
{
    public class ApplicationInsightsArgs : BaseArgs
    {
        public Input<string> WorkSpaceId { get; }
        public ApplicationInsightsArgs(BaseArgs baseArgs, Input<string> workSpaceId) : base(baseArgs)
        {
            WorkSpaceId = workSpaceId;
        }
    }
}
