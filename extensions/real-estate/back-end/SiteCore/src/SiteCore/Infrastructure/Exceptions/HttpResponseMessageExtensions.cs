using System.Text.Json;
using Willow.Api.Client;
using Willow.Infrastructure;

namespace System.Net.Http
{
    public static class HttpResponseMessageExtensions
    {
        public static void EnsureSuccessStatusCode(this HttpResponseMessage message, string dependencyServiceName)
        {
            if (!message.IsSuccessStatusCode)
            {
                var exceptionMessage = GetExceptionMessage(message);
                throw new RestException(exceptionMessage, message.StatusCode, string.Empty) { ApiName = dependencyServiceName };
            }
        }

        private static string GetExceptionMessage(HttpResponseMessage responseMessage)
        {
            try
            {
                if (responseMessage.Content.Headers.ContentType.MediaType == "application/problem+json")
                {
                    var responseString = responseMessage.Content.ReadAsStringAsync().Result;
                    var innerError = JsonSerializer.Deserialize<ErrorResponse>(responseString);
                    if (!string.IsNullOrEmpty(innerError.Message))
                    {
                        return innerError.Message;
                    }
                    else
                    {
                        return $"Unknown error response: {responseString}";
                    }
                }
                else
                {
                    return $"Error response is in media type {responseMessage.Content.Headers.ContentType.MediaType}";
                }
            }
            catch (Exception ex)
            {
                return $"Failed to get information from error response. {ex.Message}";
            }
        }
    }
}
