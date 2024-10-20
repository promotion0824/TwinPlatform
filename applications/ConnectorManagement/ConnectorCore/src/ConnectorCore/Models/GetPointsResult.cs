namespace ConnectorCore.Models
{
    using System.Collections.Generic;
    using ConnectorCore.Dtos;

    /// <summary>
    /// Represents the result of a get points operation.
    /// </summary>
    public class GetPointsResult
    {
        /// <summary>
        /// Gets or sets the list of point data transfer objects.
        /// </summary>
        public List<PointDto> Data { get; set; }

        /// <summary>
        /// Gets or sets the continuation token for pagination.
        /// </summary>
        public string ContinuationToken { get; set; }
    }
}
