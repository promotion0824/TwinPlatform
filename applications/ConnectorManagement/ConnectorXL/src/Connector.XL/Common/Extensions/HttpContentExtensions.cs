namespace Connector.XL.Common.Extensions;

using System.Text.Json;
using System.Text.Json.Serialization;

internal static class HttpContentExtensions
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase),
        },
    };

    public static async Task<T> ReadAsAsync<T>(this HttpContent content)
    {
        var stream = await content.ReadAsStreamAsync();
        return await JsonSerializer.DeserializeAsync<T>(stream, JsonSerializerOptions);
    }
}
