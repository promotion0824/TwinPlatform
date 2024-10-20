namespace Willow.Model.Requests;

/// <summary>
/// Record structure to create document twin request
/// </summary>
/// <param name="FileName">File name of the document.</param>
/// <param name="UserEmail">Email Address of the user uploading the document.</param>
/// <param name="UniqueId">Generated unique GUID.</param>
/// <param name="SiteId">Site Id association for the document twin.Can be empty if not tagged to a location.</param>
/// <param name="BlobPath">Blob path of the document.</param>
public record CreateDocumentTwinRequest(string FileName, string UserEmail, string UniqueId, string? SiteId, string BlobPath);
