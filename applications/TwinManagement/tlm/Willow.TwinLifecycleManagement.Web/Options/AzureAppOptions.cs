namespace Willow.TwinLifecycleManagement.Web.Options
{
	public class AzureAppOptions
	{
		public const string Config = "AZUREAPPLICATION";

		public string ClientId { get; set; }

		public string BaseUrl { get; set; }

		public string[] BackendB2CScopes { get; set; }

		public string[] FrontendB2CScopes { get; set; }

		public string[] KnownAuthorities { get; set; }

		public string Authority { get; set; }

	}
}
