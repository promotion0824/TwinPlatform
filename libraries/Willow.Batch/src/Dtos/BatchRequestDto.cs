namespace Willow.Batch
{
    using System;

    /// <summary>
    /// A batch request.
    /// </summary>
    public class BatchRequestDto
    {
        /// <summary>
        /// Gets the specifications on how to sort the batch.
        /// </summary>
        public SortSpecificationDto[] SortSpecifications { get; init; } = Array.Empty<SortSpecificationDto>();

        /// <summary>
        /// Gets or sets the specification on how to filter the batch.
        /// </summary>
        public FilterSpecificationDto[] FilterSpecifications { get; set; } = Array.Empty<FilterSpecificationDto>();

        /// <summary>
        /// Gets the page number to return for the batch (one-based).
        /// </summary>
        public int? Page { get; init; }

        /// <summary>
        /// Gets the amount of items in the batch.
        /// </summary>
        public int? PageSize { get; init; }
    }
}
