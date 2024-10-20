namespace Willow.PublicApi.Tests.TwinPermissionTransform;

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging.Abstractions;
using Willow.PublicApi.Authorization;
using Willow.PublicApi.Transforms;
using Yarp.ReverseProxy.Transforms;
using Yarp.ReverseProxy.Transforms.Builder;

[Binding]
[Scope(Feature = "Twin ID Query Transform")]
public class TwinIDQueryTransformStepDefinitions(ScenarioContext context)
{
    private readonly Dictionary<string, string> transformValues = [];
    private readonly TransformBuilderContext builderContext = new();
    private readonly Mock<IAuthorizationService> authorizationServiceMock = new();
    private string proxyRequestQuery;

    [Given(@"I have a query transform with ID ""([^""]*)""")]
    public void GivenIHaveAQueryTransformWithId(string twinId)
    {
        transformValues.Add("TwinIdQuery", twinId);
    }

    [When(@"I validate the query transform")]
    public void WhenIValidateTheQueryTransform()
    {
        TwinPermissionsTransform twinPermissionsTransform = new(NullLogger<TwinPermissionsTransform>.Instance, null, null);
        var result = twinPermissionsTransform.Validate(null, transformValues);
        context.Set(result, SharedStepDefinitions.ResultKey);
    }

    [When(@"I build the query transform")]
    public void WhenIBuildTheQueryTransform()
    {
        TwinPermissionsTransform twinPermissionsTransform = new(NullLogger<TwinPermissionsTransform>.Instance, null, null);
        var result = twinPermissionsTransform.Build(builderContext, transformValues);
        context.Set(result, SharedStepDefinitions.ResultKey);
    }

    [When(@"I execute the query transform with ""([^""]*)""")]
    public async Task WhenIExecuteTheQueryTransformWith(string twinId)
    {
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

        var queryParam = transformValues["TwinIdQuery"];

        HttpContext httpContext = new DefaultHttpContext();

        httpContext.Request.Scheme = "http";
        httpContext.Request.Host = new HostString("localhost");
        httpContext.Request.Path = $"/twins";
        httpContext.Request.Query = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
        {
            { queryParam, twinId },
        });

        RequestTransformContext requestTransformContext = new()
        {
            HttpContext = httpContext,
            ProxyRequest = new HttpRequestMessage(),
        };

        await builderContext.RequestTransforms.First().ApplyAsync(requestTransformContext);

        context.Set(httpContext.Response.StatusCode, SharedStepDefinitions.ResultKey);
    }

    [When(@"I execute the query transform with multiple values ""([^""]*)""")]
    public async Task WhenIExecuteTheQueryTransformWith(string[] twinIds)
    {
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

        var queryParam = transformValues["TwinIdQuery"];

        HttpContext httpContext = new DefaultHttpContext();

        httpContext.Request.Scheme = "http";
        httpContext.Request.Host = new HostString("localhost");
        httpContext.Request.Path = $"/twins";
        httpContext.Request.Query = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
        {
            { queryParam, twinIds },
        });

        RequestTransformContext requestTransformContext = new()
        {
            HttpContext = httpContext,
            ProxyRequest = new HttpRequestMessage(HttpMethod.Get, $"http://backend/twins{httpContext.Request.QueryString}"),
        };

        await builderContext.RequestTransforms.First().ApplyAsync(requestTransformContext);

        proxyRequestQuery = requestTransformContext.Query.QueryString.Value;

        context.Set(httpContext.Response.StatusCode, SharedStepDefinitions.ResultKey);
    }

    [Then(@"the query string for ""([^""]*)"" will have values ""([^""]*)""")]
    public void ThenTheQueryStringForWillHaveValues(string queryKey, string[] expectedTwinIds)
    {
        Assert.NotEmpty(proxyRequestQuery);

        var queryParams = QueryHelpers.ParseQuery(proxyRequestQuery);

        Assert.True(queryParams.ContainsKey(queryKey));

        var queryValues = queryParams[queryKey].ToArray().OrderBy(s => s);

        Assert.Collection(queryValues, expectedTwinIds.OrderBy(s => s).Select<string, Action<string>>(expected => actual => Assert.Equal(expected, actual)).ToArray());
    }
}
