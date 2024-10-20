namespace Willow.AzureDigitalTwins.Api.Messaging.Configuration
{
	public static class ChangeEvent
	{
		public const string MessageCloudEventsType = "cloudEvents:type";
		public const string MessageCloudEventsDeleteAction = "Delete";
		public const string MessageCloudEventsUpdateAction = "Update";
		public const string MessageCloudEventsCreateAction = "Create";
		public const string MessageCloudEventsSubject = "cloudEvents:subject";
		public const string MessageTwinEventNamespace = "Microsoft.DigitalTwins.Twin";
		public const string MessageRelationshipEventNamespace = "Microsoft.DigitalTwins.Relationship";
		public const string MessageModelEventNamespace = "Microsoft.DigitalTwins.Model";
	}
}
