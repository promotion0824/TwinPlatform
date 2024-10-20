
namespace Willow.AzureDigitalTwins.SDK.Client
{
	partial class ModelsClient
	{

		static partial void UpdateJsonSerializerSettings(System.Text.Json.JsonSerializerOptions settings)
		{
			if (settings == null)
				return;
			settings.PropertyNameCaseInsensitive = true;
		}

		partial void ProcessResponse(HttpClient client, HttpResponseMessage response)
		{
			this.ReadResponseAsString = true;
		}
	}
}
