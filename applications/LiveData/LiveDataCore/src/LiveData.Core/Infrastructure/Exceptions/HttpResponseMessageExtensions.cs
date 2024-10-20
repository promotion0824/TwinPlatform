namespace System.Net.Http;

using Willow.Infrastructure.Exceptions;

internal static class HttpResponseMessageExtensions
{
    public static void EnsureSuccessStatusCode(this HttpResponseMessage message, string dependencyServiceName)
    {
        if (message.StatusCode == HttpStatusCode.BadRequest)
        {
            var content = message.Content.ReadAsStringAsync().Result;
            throw new BadRequestException(content);
        }

        if (!message.IsSuccessStatusCode)
        {
            throw new DependencyServiceFailureException(dependencyServiceName, message.StatusCode);
        }
    }
}
