using Pulumi;

namespace RulesEngine.Pulumi.Components
{
    public class BaseArgs
    {
        public BaseArgs(string name, Input<string> resourceGroup, InputMap<string> tags)
        {
            Name = name;
            ResourceGroup = resourceGroup;
            Tags = tags;
        }

        protected BaseArgs(BaseArgs baseArgs)
        {
	        Name = baseArgs.Name;
	        ResourceGroup = baseArgs.ResourceGroup;
	        Tags = baseArgs.Tags;
        }

        public string Name { get; }
        public Input<string> ResourceGroup { get; }
        public InputMap<string> Tags { get; }
        public int LogRetention { get; } = 90;
    }
}
