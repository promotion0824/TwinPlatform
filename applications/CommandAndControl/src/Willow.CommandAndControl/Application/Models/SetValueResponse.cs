namespace Willow.CommandAndControl.Application.Models;

/// <summary>
/// Response model for setting a value.
/// </summary>
public class SetValueResponse
{
    /// <summary>
    /// Gets or sets the status code of the response.
    /// </summary>
    public HttpStatusCode StatusCode { get; set; }

    /// <summary>
    /// Gets or sets the ID of the point.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the result.
    /// </summary>
    public string Result { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the error.
    /// </summary>
    public Error Error { get; set; } = new();
}

/// <summary>
/// A response error.
/// </summary>
public record Error
{
    /// <summary>
    /// Gets or sets the error message.
    /// </summary>
    public string Description { get; set; } = string.Empty;
}
