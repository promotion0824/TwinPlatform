namespace Willow.PublicApi.Tests.TwinPermissionTransform;

using System.Linq;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Willow.PublicApi.Authorization;
using Willow.PublicApi.Transforms;
using Yarp.ReverseProxy.Transforms;
using Yarp.ReverseProxy.Transforms.Builder;

[Binding]
[Scope(Feature = "Twin ID and External ID Body Transform")]
public class TwinIdBodyTransformWithExternalIdStepDefinitions(ScenarioContext context)
{
    private readonly Dictionary<string, string> transformValues = [];
    private readonly TransformBuilderContext builderContext = new();
    private readonly Mock<IAuthorizationService> authorizationServiceMock = new();
    private HttpRequestMessage proxyRequest;
    private JsonDocument proxyRequestJson;

    [BeforeScenario]
    private void SetupAuthorizationMock()
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
    }

    [Given(@"I have a body transform with ID ""([^""]*)"" and external ID ""([^""]*)""")]
    public void GivenIHaveABodyTransformWithId(string bodyParam, string externalIdParam)
    {
        transformValues["TwinIdBody"] = bodyParam;
        transformValues["ExternalIdBody"] = externalIdParam;
    }

    [When(@"I validate the body transform")]
    public void WhenIValidateTheBodyTransform()
    {
        TwinPermissionsTransform twinPermissionsTransform = new(NullLogger<TwinPermissionsTransform>.Instance, null, null);
        var result = twinPermissionsTransform.Validate(null, transformValues);
        context.Set(result, SharedStepDefinitions.ResultKey);
    }

    [When(@"I build the body transform")]
    public void WhenIBuildTheBodyTransform()
    {
        TwinPermissionsTransform twinPermissionsTransform = new(NullLogger<TwinPermissionsTransform>.Instance, null, null);
        var result = twinPermissionsTransform.Build(builderContext, transformValues);
        context.Set(result, SharedStepDefinitions.ResultKey);
    }

    [When(@"I execute the body transform with multiple values ""([^""]*)"" and external IDs ""([^""]*)""")]
    public async Task WhenIExecuteTheBodyTransform(string[] twinIds, string[] externalIds)
    {
        TwinPermissionsTransform twinPermissionsTransform = new(NullLogger<TwinPermissionsTransform>.Instance, authorizationServiceMock.Object, context.Get<Mock<IResourceChecker>>().Object);
        var result = twinPermissionsTransform.Build(builderContext, transformValues);
        Assert.True(result);

        var bodyParam = transformValues["TwinIdBody"];
        var externalIdParam = transformValues["ExternalIdBody"];

        Assert.False(string.IsNullOrEmpty(bodyParam));
        Assert.False(string.IsNullOrEmpty(externalIdParam));

        HttpContext httpContext = CreateHttpContext();

        JsonNode root = new JsonObject
        {
            [bodyParam] = new JsonArray(twinIds.Select(t => JsonValue.Create<string>(t)).ToArray()),
            [externalIdParam] = new JsonArray(externalIds.Select(e => JsonValue.Create<string>(e)).ToArray()),
        };

        httpContext.Request.Body = await JsonContent.Create(root).ReadAsStreamAsync();
        httpContext.Request.Body.Position = 0;

        RequestTransformContext requestTransformContext = new()
        {
            HttpContext = httpContext,
            ProxyRequest = new HttpRequestMessage()
            {
                Content = new StreamContent(httpContext.Request.Body),
            },
        };

        await builderContext.RequestTransforms.First().ApplyAsync(requestTransformContext);

        proxyRequest = requestTransformContext.ProxyRequest;
        await GetProxyRequestJson();
        context.Set(httpContext.Response.StatusCode, SharedStepDefinitions.ResultKey);
    }

    [When(@"I execute the body transform with a single value ""([^""]*)"" and external ID ""([^""]*)""")]
    public async Task WhenIExecuteTheBodyTransformWithASingleValue(string twinId, string externalId)
    {
        TwinPermissionsTransform twinPermissionsTransform = new(NullLogger<TwinPermissionsTransform>.Instance, authorizationServiceMock.Object, context.Get<Mock<IResourceChecker>>().Object);
        var result = twinPermissionsTransform.Build(builderContext, transformValues);
        Assert.True(result);

        var bodyParam = transformValues["TwinIdBody"];
        var externalIdParam = transformValues["ExternalIdBody"];

        Assert.False(string.IsNullOrEmpty(bodyParam));
        Assert.False(string.IsNullOrEmpty(externalIdParam));

        HttpContext httpContext = CreateHttpContext();

        JsonNode root = new JsonObject
        {
            [bodyParam] = JsonValue.Create(twinId),
            [externalIdParam] = JsonValue.Create(externalId),
        };

        httpContext.Request.Body = await JsonContent.Create(root).ReadAsStreamAsync();
        httpContext.Request.Body.Position = 0;

        RequestTransformContext requestTransformContext = new()
        {
            HttpContext = httpContext,
            ProxyRequest = new HttpRequestMessage()
            {
                Content = new StreamContent(httpContext.Request.Body),
            },
        };

        await builderContext.RequestTransforms.First().ApplyAsync(requestTransformContext);

        proxyRequest = requestTransformContext.ProxyRequest;
        await GetProxyRequestJson();
        context.Set(httpContext.Response.StatusCode, SharedStepDefinitions.ResultKey);
    }

    [When(@"I execute the body transform with an array of objects with the single value ""([^""]*)"" and external ID ""([^""]*)""")]
    public async Task WhenIExecuteTheBodyTransformWithAnArrayOfObjectsWithTheSingleValue(string[] twinIds, string[] externalIds)
    {
        int maxLength = Math.Max(twinIds.Length, externalIds.Length);

        var twinIdsPadded = twinIds.Concat(Enumerable.Repeat((string)null, maxLength - twinIds.Length)).ToArray();
        var externalIdsPadded = externalIds.Concat(Enumerable.Repeat((string)null, maxLength - externalIds.Length)).ToArray();

        (string TwinId, string ExternalId)[] ids = twinIdsPadded.Zip(externalIdsPadded, (t, e) => (t, e)).ToArray();

        TwinPermissionsTransform twinPermissionsTransform = new(NullLogger<TwinPermissionsTransform>.Instance, authorizationServiceMock.Object, context.Get<Mock<IResourceChecker>>().Object);
        var result = twinPermissionsTransform.Build(builderContext, transformValues);
        Assert.True(result);

        var bodyParam = transformValues["TwinIdBody"];
        var externalIdParam = transformValues["ExternalIdBody"];

        Assert.False(string.IsNullOrEmpty(bodyParam));
        Assert.False(string.IsNullOrEmpty(externalIdParam));

        HttpContext httpContext = CreateHttpContext();

        JsonNode root = new JsonArray(ids.Select(t => new JsonObject(new Dictionary<string, JsonNode>
            {
                { bodyParam, JsonValue.Create<string>(t.TwinId) },
                { externalIdParam, JsonValue.Create<string>(t.ExternalId) },
            })).ToArray());

        httpContext.Request.Body = await JsonContent.Create(root).ReadAsStreamAsync();
        httpContext.Request.Body.Position = 0;

        RequestTransformContext requestTransformContext = new()
        {
            HttpContext = httpContext,
            ProxyRequest = new HttpRequestMessage()
            {
                Content = new StreamContent(httpContext.Request.Body),
            },
        };

        await builderContext.RequestTransforms.First().ApplyAsync(requestTransformContext);

        proxyRequest = requestTransformContext.ProxyRequest;
        await GetProxyRequestJson();
        context.Set(httpContext.Response.StatusCode, SharedStepDefinitions.ResultKey);
    }

    [Then(@"the JSON property for ""([^""]*)"" will have values ""([^""]*)""")]
    public void ThenTheJSONPropertyForWillHaveValues(string bodyParam, string[] expectedTwinIds)
    {
        JsonElement element;

        if (string.IsNullOrEmpty(bodyParam))
        {
            element = proxyRequestJson.RootElement;
        }
        else
        {
            proxyRequestJson.RootElement.TryGetProperty(bodyParam, out element);
        }

        Assert.Equal(JsonValueKind.Array, element.ValueKind);
        var actual = element.EnumerateArray().Select(e => e.GetString()).OrderBy(t => t);
        Assert.Collection(actual, expectedTwinIds.Select<string, Action<string>>(expected => actual => Assert.Equal(expected, actual)).ToArray());
    }

    [Then(@"the JSON property for ""([^""]*)"" will have the value ""([^""]*)""")]
    public void ThenTheJSONPropertyForWillHaveTheValue(string bodyParam, string expectedTwinId)
    {
        proxyRequestJson.RootElement.TryGetProperty(bodyParam, out JsonElement element);

        Assert.Equal(JsonValueKind.String, element.ValueKind);
        Assert.Equal(expectedTwinId, element.GetString());
    }

    [Then(@"the array of objects will have JSON properties for ""([^""]*)"" and will have the values ""([^""]*)""")]
    public void ThenTheArrayOfObjectsWillHaveJSONPropertiesForAndWillHaveTheValues(string twinId, string[] expectedTwinIds)
    {
        Assert.Equal(JsonValueKind.Array, proxyRequestJson.RootElement.ValueKind);

        List<string> actualTwinIds = [];

        foreach (var element in proxyRequestJson.RootElement.EnumerateArray())
        {
            Assert.Equal(JsonValueKind.Object, element.ValueKind);
            Assert.True(element.TryGetProperty(twinId, out JsonElement twinIdElement));

            if (twinIdElement.ValueKind == JsonValueKind.Null)
            {
                actualTwinIds.Add(string.Empty);
            }
            else
            {
                Assert.Equal(JsonValueKind.String, twinIdElement.ValueKind);
                actualTwinIds.Add(twinIdElement.GetString());
            }
        }

        Assert.Collection(actualTwinIds.OrderBy(t => t), expectedTwinIds.OrderBy(t => t).Select<string, Action<string>>(expected => actual => Assert.Equal(expected, actual)).ToArray());
    }

    private static HttpContext CreateHttpContext()
    {
        HttpContext httpContext = new DefaultHttpContext();

        httpContext.Request.Scheme = "http";
        httpContext.Request.Host = new HostString("localhost");
        httpContext.Request.Path = $"/twins";
        httpContext.Request.EnableBuffering();
        httpContext.Request.Body = new MemoryStream();

        return httpContext;
    }

    private async Task GetProxyRequestJson()
    {
        var content = proxyRequest.Content as StreamContent;

        Assert.NotNull(content);

        proxyRequestJson = await JsonDocument.ParseAsync(content.ReadAsStream());
    }
}
