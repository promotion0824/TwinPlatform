using Pulumi;
using Pulumi.AzureNative.ServiceBus;
using Pulumi.AzureNative.ServiceBus.Inputs;
using RulesEngine.Pulumi.Components.Helpers;
using RulesEngine.Pulumi.Components.Tags;

namespace RulesEngine.Pulumi.Components.ServiceBus;

public class ServiceBus  : ComponentResource
{
	public ServiceBus(ServiceBusArgs args) : base($"{StringConstants.ComponentNamespace}:component:ServiceBus", args.Name)
	{

		var serviceBusNamespace = new Namespace($"{args.Name}-sb", new NamespaceArgs
		{
			ResourceGroupName = args.ResourceGroup,
			NamespaceName = $"{args.Name}-sb-ns",
			Sku = new SBSkuArgs
			{
				Name = SkuName.Standard,
				Tier = SkuTier.Standard
			},
			Tags = args.Tags
		}, new CustomResourceOptions {Parent = this, IgnoreChanges = TagConstants.TagChangesToIgnore.ToList(), Protect = true});


		var requestTopic = new Topic($"{args.Name}-request", new TopicArgs
		{
			AutoDeleteOnIdle = "P6M",
			TopicName = $"{args.Name}-request",
			NamespaceName = serviceBusNamespace.Name,
			ResourceGroupName = args.ResourceGroup,
			MaxSizeInMegabytes = 1024
		}, new CustomResourceOptions {Parent = serviceBusNamespace, IgnoreChanges = TagConstants.TagChangesToIgnore.ToList()});

		var requestSubscription = new Subscription($"{args.Name}-request-subscription", new SubscriptionArgs
		{
			DefaultMessageTimeToLive = "P1D",
			SubscriptionName = $"{args.Name}-request-subscription",
			MaxDeliveryCount = 1,       // required
			TopicName = requestTopic.Name,
			NamespaceName = serviceBusNamespace.Name,
			ResourceGroupName = args.ResourceGroup
			//LockDuration = "P0DT0H5M0S",  // 5 minutes (the max)
			//DefaultMessageTtl = "P0DT0H5M0S",  // 5 minutes if not set on message itself
			//AutoDeleteOnIdle = "P6M", // Six months
		}, new CustomResourceOptions {Parent = requestTopic, IgnoreChanges = TagConstants.TagChangesToIgnore.ToList()});

		var responseTopic = new Topic($"{args.Name}-response", new TopicArgs
		{
			TopicName = $"{args.Name}-response",
			NamespaceName = serviceBusNamespace.Name,
			ResourceGroupName = args.ResourceGroup,
			MaxSizeInMegabytes = 1024
		}, new CustomResourceOptions {Parent = serviceBusNamespace, IgnoreChanges = TagConstants.TagChangesToIgnore.ToList()});


		var responseSubscription = new Subscription($"{args.Name}-response-subscription", new SubscriptionArgs
		{
			SubscriptionName = $"{args.Name}-response-subscription",
			DefaultMessageTimeToLive = "P1D",
			MaxDeliveryCount = 1,       // required
			TopicName = responseTopic.Name,
			NamespaceName = serviceBusNamespace.Name,
			ResourceGroupName = args.ResourceGroup
			//LockDuration = "P0DT0H5M0S",  // 5 minutes (the max)
			//DefaultMessageTtl = "P0DT0H5M0S",  // 5 minutes if not set on message itself
			//AutoDeleteOnIdle = "P6M", // Six months
		}, new CustomResourceOptions {Parent = responseTopic, IgnoreChanges = TagConstants.TagChangesToIgnore.ToList()});
	}
}