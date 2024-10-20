namespace Willow.ServiceBus;

/// <summary>
/// The message processing result.
/// </summary>
public class MessageProcessingResult
{
    private MessageProcessingResult()
    {
    }

    /// <summary>
    /// Gets a value indicating whether the processing was successful.
    /// </summary>
    public bool IsSuccessful { get; init; }

    /// <summary>
    /// Gets the description of the processing result.
    /// </summary>
    public string? Description { get; private init; }

    /// <summary>
    /// Creates a successful message processing result.
    /// </summary>
    /// <param name="description">The description to assign to the result.</param>
    /// <returns>A message processing result.</returns>
    public static MessageProcessingResult Success(string? description = null)
    {
        return new() { IsSuccessful = true, Description = description };
    }

    /// <summary>
    /// Creates a failed message processing result.
    /// </summary>
    /// <param name="errorDescription">The description to assign to the result.</param>
    /// <returns>A message processing result.</returns>
    public static MessageProcessingResult Failed(string errorDescription)
    {
        return new() { IsSuccessful = false, Description = errorDescription };
    }
}
