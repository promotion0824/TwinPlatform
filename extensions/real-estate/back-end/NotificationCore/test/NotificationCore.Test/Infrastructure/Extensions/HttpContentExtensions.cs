
using System.Text.Json;
using Willow.ExceptionHandling;
using Willow.Infrastructure;

namespace NotificationCore.Test.Infrastructure.Extensions;

public static class HttpContentExtensions
{
    public static async Task<ErrorResponse> ReadAsErrorResponseAsync(this HttpContent content)
    {
        var s = await content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ErrorResponse>(s, JsonSerializerExtensions.DefaultOptions);
    }
}
