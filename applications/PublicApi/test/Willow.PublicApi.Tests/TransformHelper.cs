namespace Willow.PublicApi.Tests;

using Moq;
using Willow.PublicApi.Services;
using Yarp.ReverseProxy.Transforms;

internal static class TransformHelper
{
    public static Mock<RequestTransformContext> GetRequestTransformMock(string uri, HttpContent httpContent)
    {
        Mock<RequestTransformContext> mock = new();
        mock.SetupAllProperties();

        mock.Setup(mock => mock.HttpContext.Request.Path).Returns(uri);
        mock.Setup(mock => mock.ProxyRequest.Content).Returns(httpContent);

        return mock;
    }

    public static Mock<IClientIdAccessor> GetClientIdAccessorMock(string clientId = "ClientId")
    {
        Mock<IClientIdAccessor> mock = new();
        mock.SetupAllProperties();

        mock.Setup(mock => mock.GetClientId()).Returns(clientId);
        mock.Setup(mock => mock.TryGetClientId(out It.Ref<string>.IsAny))
            .Callback((out string clientId) =>
            {
                clientId = "ClientId";
            })
            .Returns(true);

        return mock;
    }
}
