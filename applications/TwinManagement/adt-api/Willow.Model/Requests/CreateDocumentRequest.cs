using Azure.DigitalTwins.Core;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Willow.Model.Requests;

// Commented out due to TLM Upload documents endpoint fails to hit ADT documents upload POST endpoint with this annotation
// [ModelBinder(typeof(RequestFormFileModelBinder), Name = "data")]
public class CreateDocumentRequest
{
    public CreateDocumentRequest()
    {
        ShareStorageForSameFile = true;
    }

    [Required(ErrorMessage = "ERR_INVALID_TWIN")]
    public BasicDigitalTwin? Twin { get; set; }

    public bool ShareStorageForSameFile { get; set; }

    [Required(ErrorMessage = "ERR_INVALID_FORM_FILE")]
    public IFormFile? FormFile { get; set; }

    [Required(ErrorMessage = "ERR_INVALID_USER_EMAIL")]
    public string? UserEmail { get; set; }

    [Required(ErrorMessage = "ERR_INVALID_UNIQUE_ID")]
    public string? UniqueId { get; set; }

    public string SiteId { get; set; } = string.Empty;
}
