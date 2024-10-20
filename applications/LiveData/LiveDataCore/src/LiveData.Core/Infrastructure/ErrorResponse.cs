namespace Willow.Infrastructure;

/// <summary>
/// Response object for error messages.
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// Gets or sets the status code.
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Gets or sets the error message.
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// Gets or sets any data related to the error.
    /// </summary>
    public object Data { get; set; }

    /// <summary>
    /// Gets or sets the call stack.
    /// </summary>
    public string[] CallStack { get; set; }
}
