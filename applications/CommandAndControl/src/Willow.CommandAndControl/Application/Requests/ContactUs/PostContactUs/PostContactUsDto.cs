namespace Willow.CommandAndControl.Application.Requests.ContactUs.PostContactUs;

/// <summary>
/// Contact us.
/// </summary>
public record PostContactUsDto
{
    /// <summary>
    /// Gets the requester's name.
    /// </summary>
    public required string RequestersName { get; init; }

    /// <summary>
    /// Gets the requester's email.
    /// </summary>
    public required string RequestersEmail { get; init; }

    /// <summary>
    /// Gets the comment.
    /// </summary>
    public required string Comment { get; init; }

    /// <summary>
    /// Gets the URL of the page the user was on.
    /// </summary>
    public required string Url { get; init; }
}
