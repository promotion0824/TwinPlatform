using System.Text.Json;
using Willow.ExceptionHandling;
using Willow.ExceptionHandling.Exceptions;
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
                throw new DependencyServiceFailureException(dependencyServiceName, message.StatusCode, exceptionMessage);
            }
        }

        private static string GetExceptionMessage(HttpResponseMessage responseMessage)
        {
            try
            {
                if (responseMessage.Content.Headers.ContentType.MediaType == "application/problem+json")
                {
                    var responseString = responseMessage.Content.ReadAsStringAsync().Result;
                    var innerError = JsonSerializer.Deserialize<ErrorResponse>(responseString, JsonSerializerExtensions.DefaultOptions);
                    return !string.IsNullOrEmpty(innerError.Message) ? innerError.Message : $"Unknown error response: {responseString}";
                }

                return $"Error response is in media type {responseMessage.Content.Headers.ContentType.MediaType}";
            }
            catch(Exception ex)
            {
                return $"Failed to get information from error response. {ex.Message}";
            }
        }
    }
}
