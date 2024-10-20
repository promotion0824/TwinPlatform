using System.ComponentModel.DataAnnotations;
using Willow.DataValidation;

namespace Willow.Workflow
{
    public class CreateInspectionZoneRequest
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Name is required")]
        [HtmlContent]
        [StringLength(200)]
        public string Name { get; set; }
    }
}
