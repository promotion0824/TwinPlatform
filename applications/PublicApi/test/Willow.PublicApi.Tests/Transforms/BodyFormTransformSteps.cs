namespace Willow.PublicApi.Tests.Transforms;

using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TechTalk.SpecFlow;
using Willow.PublicApi.Transforms;
using Xunit;
using Yarp.ReverseProxy.Transforms;
using Yarp.ReverseProxy.Transforms.Builder;

[Binding]
[Scope(Feature = "Body Form Transform")]
public class BodyFormTransformSteps(ScenarioContext scenarioContext)
{
    private readonly TransformBuilderContext transformBuilderContext = new();
    private readonly BodyFormTransform bodyFormTransform = new();
    private readonly DefaultHttpContext httpContext = new();
    private IReadOnlyDictionary<string, string> transformValues;

    [Given(@"the transform values contain ""([^""]*)"" with value ""([^""]*)"" and ""([^""]*)"" with value ""([^""]*)""")]
    public void GivenTheTransformValuesContainWithValueAndWithValue(string key1, string value1, string key2, string value2)
    {
        transformValues = new Dictionary<string, string>
        {
            { key1, value1 },
            { key2, value2 },
        };
    }

    [Given(@"the transform values contain ""([^""]*)"" with value ""([^""]*)""")]
    public void GivenTheTransformValuesContainWithValue(string key, string value)
    {
        transformValues = new Dictionary<string, string>
        {
            { key, value },
        };
    }

    [Given(@"the HTTP context has form content type with existing form data")]
    public void GivenTheHttpContextHasFormContentTypeWithExistingFormData()
    {
        httpContext.Request.ContentType = "application/x-www-form-urlencoded";
        var formCollection = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
        {
            { "existingKey", "existingValue" },
        });
        httpContext.Request.Form = formCollection;
    }

    [When(@"the transform is built")]
    public async Task WhenTheTransformIsBuilt()
    {
        var result = bodyFormTransform.Build(transformBuilderContext, transformValues);
        scenarioContext["BuildResult"] = result;

        if (result)
        {
            var transformContext = new RequestTransformContext
            {
                HttpContext = httpContext,
                ProxyRequest = new HttpRequestMessage(),
            };

            foreach (var transform in transformBuilderContext.RequestTransforms)
            {
                await transform.ApplyAsync(transformContext);
            }

            scenarioContext["FormData"] = await transformContext.ProxyRequest.Content.ReadAsStringAsync();
        }
    }

    [When(@"the transform is validated")]
    public void WhenTheTransformIsValidated()
    {
        var result = bodyFormTransform.Validate(null, transformValues);
        scenarioContext["ValidationResult"] = result;
    }

    [Then(@"the form data should contain ""([^""]*)"" with value ""([^""]*)""")]
    public void ThenTheFormDataShouldContainWithValue(string key, string value)
    {
        var formData = scenarioContext["FormData"] as string;
        Assert.Contains($"{key}={value}", formData);
    }

    [Then(@"the result should be false")]
    public void ThenTheResultShouldBeFalse()
    {
        var result = (bool)scenarioContext["ValidationResult"];
        Assert.False(result);
    }

    [Then(@"the result should be true")]
    public void ThenTheResultShouldBeTrue()
    {
        var result = (bool)scenarioContext["ValidationResult"];
        Assert.True(result);
    }
}
