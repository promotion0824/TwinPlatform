using System.Text.Json;
using Willow.Infrastructure;
using Willow.Infrastructure.Exceptions;

namespace System.Net.Http
{
    public static class HttpResponseMessageExtensions
    {
        public static void EnsureSuccessStatusCode(
            this HttpResponseMessage message,
            string dependencyServiceName
        )
        {
            if (!message.IsSuccessStatusCode)
            {
                var exceptionMessage = GetExceptionMessage(message);
                throw new DependencyServiceFailureException(
                    dependencyServiceName,
                    message.StatusCode,
                    exceptionMessage
                );
            }
        }

        private static string GetExceptionMessage(HttpResponseMessage responseMessage)
        {
            try
            {
                if (
                    responseMessage.Content.Headers.ContentType.MediaType
                    == "application/problem+json"
                )
                {
                    var responseString = responseMessage.Content.ReadAsStringAsync().Result;
                    var innerError = JsonSerializerExtensions.Deserialize<ErrorResponse>(
                        responseString
                    );
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
