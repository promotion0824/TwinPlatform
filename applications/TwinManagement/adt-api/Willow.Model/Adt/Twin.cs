using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Willow.Model.Adt;

public class Twin
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "ERR_INVALID_ID")]
    public string? Id { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "ERR_INVALID_MODEL_ID")]
    public string? ModelId { get; set; }

    [JsonExtensionData]
    public IDictionary<string, object> CustomProperties { get; set; } = new Dictionary<string, object>();
}
