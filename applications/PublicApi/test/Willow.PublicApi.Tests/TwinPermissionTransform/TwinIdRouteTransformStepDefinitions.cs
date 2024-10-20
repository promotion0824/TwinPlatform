namespace Willow.PublicApi.Tests.TwinPermissionTransform;

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Willow.PublicApi.Authorization;
using Willow.PublicApi.Transforms;
using Yarp.ReverseProxy.Transforms;
using Yarp.ReverseProxy.Transforms.Builder;

[Binding]
[Scope(Feature = "Twin ID Route Transform")]
public class TwinIdRouteTransformStepDefinitions(ScenarioContext context)
{
    private readonly Dictionary<string, string> transformValues = [];
    private readonly TransformBuilderContext builderContext = new();
    private readonly Mock<IAuthorizationService> authorizationServiceMock = new();

    [Given(@"I have a route transform with ID ""([^""]*)""")]
    public void GivenIHaveARouteTransformWithId(string twinId)
    {
        transformValues.Add("TwinIdRoute", twinId);
    }

    [When(@"I validate the route transform")]
    public void WhenIValidateTheRouteTransform()
    {
        TwinPermissionsTransform twinPermissionsTransform = new(NullLogger<TwinPermissionsTransform>.Instance, null, null);
        var result = twinPermissionsTransform.Validate(null, transformValues);
        context.Set(result, SharedStepDefinitions.ResultKey);
    }

    [When(@"I build the route transform")]
    public void WhenIBuildTheRouteTransform()
    {
        TwinPermissionsTransform twinPermissionsTransform = new(NullLogger<TwinPermissionsTransform>.Instance, null, null);
        var result = twinPermissionsTransform.Build(builderContext, transformValues);
        context.Set(result, SharedStepDefinitions.ResultKey);
    }

    [When(@"I execute the route transform with ""([^""]*)""")]
    public async Task WhenIExecuteTheRouteTransform(string twinId)
    {
        // Avoids having to figure out how to instantiate a working DefaultAuthorizationHandler.
        authorizationServiceMock.Setup(mock => mock.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
        .Returns((ClaimsPrincipal user, object resource, IEnumerable<IAuthorizationRequirement> requirements) =>
        {
            var handler = new SingleTwinExpressionHandler(context.Get<Mock<IResourceChecker>>().Object, TransformHelper.GetClientIdAccessorMock().Object);
            var authContext = new AuthorizationHandlerContext(requirements.ToList(), user, resource);
            handler.HandleAsync(authContext).Wait();

            return Task.FromResult(authContext.HasSucceeded ? AuthorizationResult.Success() : AuthorizationResult.Failed());
        });

        TwinPermissionsTransform twinPermissionsTransform = new(NullLogger<TwinPermissionsTransform>.Instance, authorizationServiceMock.Object, context.Get<Mock<IResourceChecker>>().Object);
        var result = twinPermissionsTransform.Build(builderContext, transformValues);
        Assert.True(result);

        var routeParam = transformValues["TwinIdRoute"];

        HttpContext httpContext = new DefaultHttpContext();

        httpContext.Request.Scheme = "http";
        httpContext.Request.Host = new HostString("localhost");
        httpContext.Request.Path = $"/twins/{twinId}";
        httpContext.Request.RouteValues.Add(routeParam, twinId);

        RequestTransformContext requestTransformContext = new()
        {
            HttpContext = httpContext,
            ProxyRequest = new HttpRequestMessage(),
        };

        await builderContext.RequestTransforms.First().ApplyAsync(requestTransformContext);

        context.Set(httpContext.Response.StatusCode, SharedStepDefinitions.ResultKey);
    }
}
