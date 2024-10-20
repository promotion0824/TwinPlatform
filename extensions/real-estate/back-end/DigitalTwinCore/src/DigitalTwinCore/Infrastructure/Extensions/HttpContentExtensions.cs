using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Willow.Infrastructure;

namespace System.Net.Http
{
	public static class HttpContentExtensions
	{
		public static async Task<T> ReadAsAsync<T>(this HttpContent content)
		{
			return await content.ReadAsAsync<T>(CancellationToken.None);
		}

		public static async Task<T> ReadAsAsync<T>(this HttpContent content, CancellationToken cancellationToken)
		{
			var stream = await content.ReadAsStreamAsync(cancellationToken);
			var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
			{
				NumberHandling = JsonNumberHandling.Strict,
				DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
			};
			options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
			options.Converters.Add(new DateTimeConverter());
			return await JsonSerializer.DeserializeAsync<T>(stream, options, cancellationToken: cancellationToken);
		}
	}
}
