namespace Willow.LiveData.Pipeline;

/// <summary>
/// A wrapper for exceptions raised sending or receiving telemetry.
/// </summary>
public class PipelineException : ApplicationException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PipelineException"/> class.
    /// </summary>
    public PipelineException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PipelineException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public PipelineException(string? message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PipelineException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">
    /// The exception that is the cause of the current exception. If the <paramref name="innerException"/>
    /// parameter is not a null reference, the current exception is raised in a catch
    /// block that handles the inner exception.
    /// </param>
    public PipelineException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
