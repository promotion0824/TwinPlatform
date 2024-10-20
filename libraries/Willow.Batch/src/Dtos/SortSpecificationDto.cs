namespace Willow.Batch;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Sort specification.
/// </summary>
public class SortSpecificationDto
{
    /// <summary>
    /// Ascending sort.
    /// </summary>
    public const string ASC = "asc";

    /// <summary>
    /// Descending sort.
    /// </summary>
    public const string DESC = "desc";

    /// <summary>
    /// Gets or sets the field name.
    /// </summary>
    [Required]
    public string Field { get; set; }

    /// <summary>
    /// Gets or sets the sort order.
    /// </summary>
    /// <remarks>
    /// "asc", "desc" or empty.
    /// </remarks>
    public string Sort { get; set; }

    /// <summary>
    /// Gets a value indicating whether the sorting is in descending order.
    /// </summary>
    public bool IsSortDescending => Sort.ToLower() == DESC;
}
