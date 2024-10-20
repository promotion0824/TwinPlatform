namespace Willow.TwinLifecycleManagement.Web.Models;

/// <summary>
/// A request to create documents.
/// </summary>
public class CreateDocumentRequest
{
	/// <summary>
	/// Gets or sets a collection of document files.
	/// </summary>
	public IEnumerable<IFormFile> Files { get; set; }

	/// <summary>
	/// Gets or sets a side id.
	/// </summary>
	public string SiteId { get; set; }
}
