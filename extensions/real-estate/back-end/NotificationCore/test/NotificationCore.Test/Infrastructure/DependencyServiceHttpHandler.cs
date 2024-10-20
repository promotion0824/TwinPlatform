using Moq;

namespace NotificationCore.Test.Infrastructure;
public class DependencyServiceHttpHandler
{
    public Mock<HttpMessageHandler> HttpHandler { get; }
    public string BaseUrl { get; }

    public DependencyServiceHttpHandler(Mock<HttpMessageHandler> httpHandler, string baseUrl)
    {
        HttpHandler = httpHandler;
        BaseUrl = baseUrl;
    }
}
