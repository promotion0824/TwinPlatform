using System.Text.Json;
using Willow.Infrastructure;
using Willow.Infrastructure.Exceptions;

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
        public static void EnsureSuccessStatusCode(this Azure.Response response, string dependencyServiceName)
        {
            if ( ! (response.Status >= 200 && response.Status <= 299))
            {
                throw new DependencyServiceFailureException(dependencyServiceName,
                    (HttpStatusCode) response.Status, response.ToString());
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
            catch(Exception ex)
            {
                return $"Failed to get information from error response. {ex.Message}";
            }
        }
    }
}
