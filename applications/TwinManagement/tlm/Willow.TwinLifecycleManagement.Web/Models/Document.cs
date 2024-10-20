namespace Willow.TwinLifecycleManagement.Web.Models;

/// <summary>
/// Information about a document
/// </summary>
public class Document
{
	/// <summary>
	/// Gets or sets twin identifier of a document.
	/// </summary>
	public string Id { get; set; }

	/// <summary>
	/// Gets or sets unique identifier of a document.
	/// </summary>
	public string UniqueId { get; set; }

	/// <summary>
	/// Gets or sets file name of the document.
	/// </summary>
	public string FileName { get; set; }

	/// <summary>
	/// Gets or sets file name of the document.
	/// </summary>
	public string Url { get; set; }

	/// <summary>
	/// Gets or sets date when the document created.
	/// </summary>
	public DateTime? CreatedDate { get; set; }

	/// <summary>
	/// Gets or sets user of created the document.
	/// </summary>
	public string CreatedBy { get; set; }

	/// <summary>
	/// Gets or sets the document type.
	/// </summary>
	public string DocumentType { get; set; }

	/// <summary>
	/// Gets or sets the location name.
	/// </summary>
	public string SiteName { get; set; }

	/// <summary>
	/// Gets or sets location identifier.
	/// </summary>
#nullable enable
	public string? SiteId { get; set; }
}
