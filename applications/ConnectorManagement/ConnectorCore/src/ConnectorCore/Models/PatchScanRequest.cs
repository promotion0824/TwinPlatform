namespace ConnectorCore.Models
{
    using System;
    using ConnectorCore.Entities;

    /// <summary>
    /// Represents a request to patch scan details.
    /// </summary>
    public class PatchScanRequest
    {
        /// <summary>
        /// Gets or sets the scan status.
        /// </summary>
        public ScanStatus? Status { get; set; }

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the error count.
        /// </summary>
        public int? ErrorCount { get; set; }

        /// <summary>
        /// Gets or sets the start time of the scan.
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// Gets or sets the end time of the scan.
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// Determines whether the request is empty.
        /// </summary>
        /// <returns>True if the request is empty; otherwise, false.</returns>
        public bool IsEmpty() => Status == null && ErrorMessage == null && ErrorCount == null && StartTime == null &&
                                 EndTime == null;
    }
}
