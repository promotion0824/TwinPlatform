using RulesEngine.Pulumi.Stacks.Resources;


class Program
{
	static Task<int> Main() => Pulumi.Deployment.RunAsync<ResourceStack>();
}
