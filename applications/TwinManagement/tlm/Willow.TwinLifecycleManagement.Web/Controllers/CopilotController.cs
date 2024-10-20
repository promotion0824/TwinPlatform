using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Willow.Copilot.ProxyAPI;
using Willow.TwinLifecycleManagement.Web.Auth;
using Willow.TwinLifecycleManagement.Web.Models;
using Willow.TwinLifecycleManagement.Web.Options;

namespace Willow.TwinLifecycleManagement.Web.Controllers;

/// <summary>
/// Copilot Controller.
/// </summary>
/// <param name="logger">Instance of ILogger.</param>
/// <param name="copilotClient">Instance Of Copilot Client.</param>
[Route("api/[controller]")]
[ApiController]
public class CopilotController(ILogger<CopilotController> logger,
    ICopilotClient copilotClient,
    IHttpClientFactory httpClientFactory,
    IOptions<SpeechServiceOption> speechServiceOption) : ControllerBase
{
    const string FetchTokenUriFormat =
        "https://{0}.api.cognitive.microsoft.com/sts/v1.0/issueToken";
    const string SpeechServiceHeaderSubscription = "Ocp-Apim-Subscription-Key";

    /// <summary>
    /// Chat with copilot api.
    /// </summary>
    /// <param name="request">Request containing the query text.</param>
    /// <returns><see cref="CopilotChatResponse"/>.</returns>
    [Authorize(AppPermissions.CanChatWithCopilot)]
    [HttpPost("Chat")]
    public async Task<CopilotChatResponse> ChatAsync([FromBody] CopilotChatRequest request)
    {
        logger.LogInformation("Received chat request: {Request}", JsonConvert.SerializeObject(request));

        try
        {
            // Create request and set the options
            var apiRequest = request.ToApiRequest();

            var response = await copilotClient.ChatAsync(apiRequest);
            return CopilotChatResponse.FromApiResponse(response);
        }
        catch (ApiException ae)
        {
            logger.LogError(ae, "Copilot API failed to respond for request : {request}.", JsonConvert.SerializeObject(request));
            return new CopilotChatResponse() { ResponseText = ae.Message };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error connecting to copilot api : {request}.", JsonConvert.SerializeObject(request));
            throw;
        }
    }

    /// <summary>
    /// Endpoint to get the authorization token for Speech Service.
    /// </summary>
    /// <returns><see cref="SpeechAuthorizationTokenResponse"/> as response.</returns>
    [Authorize]
    [HttpGet("speechToken")]
    public async Task<SpeechAuthorizationTokenResponse> GetSpeechAuthToken()
    {
        var speechOption = speechServiceOption.Value;

        using var httpClient = httpClientFactory.CreateClient(nameof(GetSpeechAuthToken));
        httpClient.DefaultRequestHeaders.Add(SpeechServiceHeaderSubscription, speechOption.SpeechKey);
        var uriBuilder = new UriBuilder(string.Format(FetchTokenUriFormat, speechOption.Region));

        using var response = await httpClient.PostAsync(uriBuilder.Uri.AbsoluteUri, null);
        var authToken = await response.Content.ReadAsStringAsync();
        return new SpeechAuthorizationTokenResponse() { Region = speechOption.Region, Token = authToken };
    }
}
