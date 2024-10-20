using Pulumi;

namespace RulesEngine.Pulumi.Components.Storage;

public class StorageArgs: BaseArgs
{
	public StorageArgs(BaseArgs baseArgs) : base(baseArgs)
	{

	}

	public Input<string> CacheFileShareName { get; set; } = "rulesenginecache";
}