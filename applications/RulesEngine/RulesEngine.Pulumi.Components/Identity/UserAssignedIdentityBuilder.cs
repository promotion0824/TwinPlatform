using Pulumi;
using Pulumi.AzureNative.ManagedIdentity;
using RulesEngine.Pulumi.Components.Tags;

namespace RulesEngine.Pulumi.Components.Identity
{
    public class UserAssignedIdentityBuilder
    {
        private readonly string _name;
        private InputMap<string>? _tags;

        public UserAssignedIdentityBuilder(string name)
        {
            _name = name;
        }

        private Input<string> _resourceGroupName = "";
        private Resource? Parent { get; set; }

        public UserAssignedIdentityBuilder WithResourceGroup(Input<string> resourceGroupName)
        {
            _resourceGroupName = resourceGroupName;
            return this;
        }

        public UserAssignedIdentityBuilder WithTags(InputMap<string>? tags)
        {
            _tags = tags;
            return this;
        }

        public UserAssignedIdentityBuilder WithParent(Resource parent)
        {
            Parent = parent;
            return this;
        }

        public UserAssignedIdentity Build()
        {
            return new(_name, new UserAssignedIdentityArgs
            {
                Tags = _tags ?? new InputMap<string>(),
                ResourceGroupName = _resourceGroupName
            }, new CustomResourceOptions {Parent = Parent, IgnoreChanges = TagConstants.TagChangesToIgnore.ToList()});
        }
    }
}
