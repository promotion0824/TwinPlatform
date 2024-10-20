using Willow.Copilot.ProxyAPI;

namespace Willow.TwinLifecycleManagement.Web.Models;

/// <summary>
/// Copilot Chat Request option.
/// </summary>
public record CopilotChatRequestOption
{
    /// <summary>
    /// Gets or sets the hint to help choose the proper LLM.
    /// </summary>
    public string ModelHint { get; set; }

    /// <summary>
    /// Gets or sets the list of prompt hint to use to create the LLM prompt template.
    /// </summary>
    public List<string> PromptHints { get; set; }

    /// <summary>
    /// Gets or sets list of flags to control the behavior of the chat request.
    /// </summary>
    public List<string> RunFlags { get; set; }

    /// <summary>
    /// Converts tlm request option to copilot request option model.
    /// </summary>
    /// <returns><see cref="ChatRequestOptions"/>.</returns>
    internal Copilot.ProxyAPI.ChatRequestOptions ToApiRequestOption()
    {
        return new ChatRequestOptions()
        {
            Model_hint = ModelHint,
            Prompt_hints = PromptHints ?? [],
            Run_flags = RunFlags ?? [],
        };
    }
}

/// <summary>
/// Copilot Chat Request.
/// </summary>
public record CopilotChatRequest
{
    /// <summary>
    /// Gets or sets the user Input for the chat.
    /// </summary>
    public string UserInput { get; set; }

    /// <summary>
    /// Gets or sets the context for the chat.
    /// </summary>
    public CopilotContext Context { get; set; }

    /// <summary>
    /// Gets or sets the chat request options.
    /// </summary>
    public CopilotChatRequestOption Options { get; set; }

    /// <summary>
    /// Converts tlm request to copilot request model.
    /// </summary>
    /// <returns><see cref="ChatRequest"/>.</returns>
    internal ChatRequest ToApiRequest()
    {
        return new ChatRequest()
        {
            Context = Context.ToApiContext(),
            User_input = UserInput,
            Options = Options.ToApiRequestOption(),
        };
    }
}

/// <summary>
/// Copilot Chat Response.
/// </summary>
public record CopilotChatResponse
{
    /// <summary>
    /// Gets or sets the response from the LLM.
    /// </summary>
    public string ResponseText { get; set; }

    /// <summary>
    /// Converts LLM Response to tlm chat response model.
    /// </summary>
    /// <param name="response">Response as string.</param>
    /// <returns><see cref="CopilotChatResponse"/>.</returns>
    internal static CopilotChatResponse FromApiResponse(ChatResponse response)
    {
        return new CopilotChatResponse()
        {
            ResponseText = response.ResponseText,
        };
    }
}

/// <summary>
/// Copilot Chat Context.
/// </summary>
public record CopilotContext
{
    /// <summary>
    /// Gets or sets the session Id for the chat.
    /// </summary>
    public string SessionId { get; set; }

    /// <summary>
    /// Gets or sets the user name of the user. Ideally email address of the user.
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// Convert tlm copilot context to api context.
    /// </summary>
    /// <returns><see cref="Context"/>.</returns>
    internal Context ToApiContext()
    {
        return new Context()
        {
            User_name = UserName,
            Session_id = SessionId,
        };
    }
}

/// <summary>
/// Speech Authorization Token Response.
/// </summary>
public record SpeechAuthorizationTokenResponse
{
    /// <summary>
    /// Gets or sets the Authorization Token.
    /// </summary>
    public string Token { get; set; }

    /// <summary>
    /// Gets or sets the Endpoint Region.
    /// </summary>
    public string Region { get; set; }
}
