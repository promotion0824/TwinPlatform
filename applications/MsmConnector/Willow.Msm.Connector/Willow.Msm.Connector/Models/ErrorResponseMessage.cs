namespace Willow.Msm.Connector.Models
{
    /// <summary>
    /// Represents a structured error message that provides details about issues encountered during an operation.
    /// </summary>
    public class ErrorResponseMessage
    {
        /// <summary>
        /// Gets or sets the status code or keyword describing the nature of the error.
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// Gets or sets a message that provides a concise description of the error.
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// Gets or sets a detailed message elaborating on the error, providing additional context or information
        /// that could assist in diagnosing issues.
        /// </summary>
        public string? DetailedMessage { get; set; }
    }
}
