namespace ConnectorCore.Models
{
    using System.Collections.Generic;
    using ConnectorCore.Dtos;

    /// <summary>
    /// Represents the result of a get equipment operation.
    /// </summary>
    public class GetEquipmentResult
    {
        /// <summary>
        /// Gets or sets the list of equipment data.
        /// </summary>
        public List<EquipmentDto> Data { get; set; }

        /// <summary>
        /// Gets or sets the continuation token for pagination.
        /// </summary>
        public string ContinuationToken { get; set; }
    }
}
