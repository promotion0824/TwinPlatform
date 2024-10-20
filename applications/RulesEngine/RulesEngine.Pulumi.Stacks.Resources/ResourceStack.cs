using Pulumi;
using Pulumi.AzureNative.Resources;
using RulesEngine.Pulumi.Components;
using RulesEngine.Pulumi.Components.ApplicationInsights;
using RulesEngine.Pulumi.Components.Identity;
using RulesEngine.Pulumi.Components.ServiceBus;
using RulesEngine.Pulumi.Components.Sql;
using RulesEngine.Pulumi.Components.Storage;
using RulesEngine.Pulumi.Components.Tags;
using Deployment = Pulumi.Deployment;

namespace RulesEngine.Pulumi.Stacks.Resources;

public class ResourceStack : Stack
{

	public ResourceStack()
	{
		var config = new Config();

		var customer = config.Require("customer");
		var environment = config.Require("environment");
		var logAnalyticsWorkspace = config.Require("workSpaceId");
		var adminGroupSid = config.Require("adminGroupSid");
		var adminGroupName = config.Require("adminGroupName");


		var tags = new TagBuilder("RulesEngineResources", customer, environment).Build();

		var name = Deployment.Instance.StackName.Replace(".", "-");
		var resourceGroup = new ResourceGroup($"{name}-rg", new ResourceGroupArgs
		{
			Tags = tags,
			ResourceGroupName = name
		}, new CustomResourceOptions { Parent = this, IgnoreChanges = TagConstants.TagChangesToIgnore.ToList() });
		ResourceGroup = resourceGroup.Name;

		var baseArgs = new BaseArgs(name, resourceGroup.Name, tags);


		var ruleEngineIdentity = new UserAssignedIdentityBuilder($"{customer}-{environment}-id")
			.WithTags(tags)
			.WithResourceGroup(resourceGroup.Name)
			.WithParent(resourceGroup)
			.Build();

		var appInsights = new ApplicationInsights(new ApplicationInsightsArgs(baseArgs, logAnalyticsWorkspace));
		InstrumentationKey = appInsights.InstrumentationKey;
		ApplicationInsightsConnectionString = appInsights.ConnectionString;

		var sql = new AzureSql(new AzureSqlArgs(baseArgs, logAnalyticsWorkspace, adminGroupName, adminGroupSid));
		RulesEngineConnectionString = sql.RulesEngineConnectionString;

		var storage = new Storage(new StorageArgs(baseArgs));

		var serviceBus = new ServiceBus(new ServiceBusArgs(baseArgs));

		IdentityId = ruleEngineIdentity.Id;
		IdentityClientId = ruleEngineIdentity.ClientId;


	}

	[Output] public Output<string> ResourceGroup { get; set; }
	[Output] public Output<string> InstrumentationKey { get; set; }
	[Output] public Output<string> ApplicationInsightsConnectionString { get; set; }

	[Output] public Output<string> IdentityId { get; set; }
	[Output] public Output<string> IdentityClientId { get; set; }
	[Output] public Output<string> RulesEngineConnectionString { get; set; }

}