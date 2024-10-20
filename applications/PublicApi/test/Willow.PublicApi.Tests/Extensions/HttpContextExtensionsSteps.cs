namespace Willow.PublicApi.Tests.Extensions;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using TechTalk.SpecFlow;
using Willow.PublicApi.Extensions;
using Xunit;

[Binding]
public class HttpContextExtensionsSteps(ScenarioContext scenarioContext)
{
    private readonly DefaultHttpContext httpContext = new();

    [Given("the HTTP context has a valid token with client ID \"(.*)\"")]
    public void GivenTheHttpContextHasAValidTokenWithClientId(string clientId)
    {
        var token = GenerateJwtToken(clientId);
        httpContext.Request.Headers["Authorization"] = $"Bearer {token}";
    }

    [Given("the HTTP context has no authorization header")]
    public void GivenTheHttpContextHasNoAuthorizationHeader()
    {
        // No action needed, default context has no authorization header
    }

    [Given("the HTTP context has an invalid authorization header")]
    public void GivenTheHttpContextHasAnInvalidAuthorizationHeader()
    {
        httpContext.Request.Headers["Authorization"] = "Not-Bearer";
    }

    [Given("the HTTP context has a form with client ID \"(.*)\"")]
    public void GivenTheHttpContextHasAFormWithClientId(string clientId)
    {
        var formCollection = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
        {
            { "client_id", clientId },
        });
        httpContext.Request.Form = formCollection;
    }

    [Given("the HTTP context has a form without client ID")]
    public void GivenTheHttpContextHasAFormWithoutClientId()
    {
        var formCollection = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>());
        httpContext.Request.Form = formCollection;
    }

    [Given("the HTTP context is not form content type")]
    public void GivenTheHttpContextIsNotFormContentType()
    {
        httpContext.Request.ContentType = "application/json";
    }

    [When("the client ID is retrieved from the token")]
    public void WhenTheClientIdIsRetrievedFromTheToken()
    {
        var clientId = httpContext.GetClientIdFromToken();
        scenarioContext["ClientId"] = clientId;
    }

    [When("the client ID is retrieved from the body")]
    public void WhenTheClientIdIsRetrievedFromTheBody()
    {
        var clientId = httpContext.GetClientIdFromBody();
        scenarioContext["ClientId"] = clientId;
    }

    [Then("the client ID should be \"(.*)\"")]
    public void ThenTheClientIdShouldBe(string expectedClientId)
    {
        var actualClientId = scenarioContext["ClientId"] as string;
        Assert.Equal(expectedClientId, actualClientId);
    }

    [Then("the client ID should be null")]
    public void ThenTheClientIdShouldBeNull()
    {
        var actualClientId = scenarioContext["ClientId"] as string;
        Assert.Null(actualClientId);
    }

    private string GenerateJwtToken(string clientId)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
            [
                new Claim("appid", clientId),
            ]),
            Expires = DateTime.UtcNow.AddHours(1),
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
