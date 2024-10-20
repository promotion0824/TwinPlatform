
namespace Authorization.Common.Models;

/// <summary>
/// Class to hold search filter properties while fetching all records
/// </summary>
public class FilterPropertyModel
{

	/// <summary>
	/// Text to search while retrieving records. Generally the records get filtered by the name property.
	/// </summary>
	public string? SearchText { get; set; } = string.Empty;

    /// <summary>
    /// Filter query for advanced filtering logic
    /// </summary>
    public string? FilterQuery { get; set; } = string.Empty;

	/// <summary>
	/// Count of row to skip before fetch
	/// </summary>
	public int? Skip { get; set; } = null!;

	/// <summary>
	/// Count to rows to fetch after optional skip rows
	/// </summary>
	public int? Take { get; set; } = null!;
}

