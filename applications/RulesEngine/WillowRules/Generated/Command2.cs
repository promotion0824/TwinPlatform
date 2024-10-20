using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Willow.RealEstate.Command.Generated
{
	public partial class CommandClient
	{
		private string? accessToken = null;

		/// <summary>
		/// Strings as enums for the Command API
		/// </summary>
		/// <param name="settings"></param>
		partial void UpdateJsonSerializerSettings(JsonSerializerSettings settings)
		{
			settings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter(
				namingStrategy: new CamelCaseNamingStrategy()));
		}

		/// <summary>
		/// Sets the access token used by Public API
		/// </summary>
		public void SetAccessToken(string accessToken)
		{
			this.accessToken = accessToken;
		}

		partial void PrepareRequest(System.Net.Http.HttpClient client, System.Net.Http.HttpRequestMessage request, string url)
		{
			request.Headers.Add("Authorization", $"Bearer {this.accessToken}");
		}
	}
}