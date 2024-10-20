using Pulumi;
using Pulumi.AzureNative.Insights;
using Pulumi.AzureNative.Insights.Inputs;

namespace RulesEngine.Pulumi.Components.Helpers
{
    public class DiagnosticsSettingBuilder
    {
        private readonly string _name;
        private readonly Input<string> _resourceUri;
        private readonly Input<string> _workspaceId;
        private readonly int _retention;
        private readonly SortedSet<string> _categories = new();
        private Resource? Parent { get; set; }

        public DiagnosticsSettingBuilder(string name, Input<string> resourceUri, Input<string> workspaceId, int retention)
        {
            _name = name;
            _resourceUri = resourceUri;
            _workspaceId = workspaceId;
            _retention = retention;
        }

        public DiagnosticsSettingBuilder WithCategory(string category)
        {
            _categories.Add(category);
            return this;
        }

        public DiagnosticsSettingBuilder WithParent(Resource parent)
        {
            Parent = parent;
            return this;
        }


        public DiagnosticSetting Build()
        {
            return new(_name, new DiagnosticSettingArgs
            {
                Name = _name,
                WorkspaceId = _workspaceId,
                ResourceUri = _resourceUri,
                Logs = _categories.Select(c => new LogSettingsArgs
                {
                    Enabled = true,
                    Category = c,
                    RetentionPolicy = new RetentionPolicyArgs
                    {
                        Days = _retention,
                        Enabled = true
                    }
                }).ToList()
            }, new CustomResourceOptions{Parent = Parent});
        }
    }
}
