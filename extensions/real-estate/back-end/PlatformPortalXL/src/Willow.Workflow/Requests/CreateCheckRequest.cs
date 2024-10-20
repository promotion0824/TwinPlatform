using System;
using System.ComponentModel.DataAnnotations;

using Willow.DataValidation;

namespace Willow.Workflow
{
    public abstract class CheckRequest
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Name is required")]
        [HtmlContent]
        [StringLength(200, ErrorMessage = "Name exceeds the max length of 200")]
        public string       Name               { get; set; }

        [Required(ErrorMessage = "Type is required")]
        public CheckType?   Type               { get; set; }

        [StringLength(512, ErrorMessage = "TypeValue exceeds the max length of 512")]
        [UniqueStringListIf("Type", CheckType.List, "|", ErrorMessage = "TypeValue contains duplicates")]
        [RequiredIfNot("Type", CheckType.Date)]
        public string       TypeValue          { get; set; }

        [RequiredIf("Type", CheckType.Numeric, ErrorMessage = "DecimalPlaces is required")]
        [RequiredIf("Type", CheckType.Total, ErrorMessage = "DecimalPlaces is required")]
        [Range(0, 4, ErrorMessage = "DecimalPlaces must be between 1 and 4")]
        public int?         DecimalPlaces      { get; set; }

        public double?      MinValue           { get; set; }
        public double?      MaxValue           { get; set; }
        public double Multiplier { get; set; } = 1;
        public string       DependencyName     { get; set; }

        [StringLength(50, ErrorMessage = "DependencyValue exceeds the max length of 50")]
        [RequiredIfNotEmpty("DependencyName", ErrorMessage = "DependencyValue is required when DependencyName is specified")]
        public string       DependencyValue    { get; set; }

        public DateTime?    PauseStartDate     { get; set; }
        public DateTime?    PauseEndDate       { get; set; }
        public bool         CanGenerateInsight { get; set; }
    }    
    
    public class CreateCheckRequest : CheckRequest
    {
    }

    public class UpdateCheckRequest : CheckRequest
    {
        public Guid? Id           { get; set; }
        public Guid? DependencyId { get; set; }
    }}
