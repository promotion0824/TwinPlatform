using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using Willow.DataValidation;

namespace Willow.Workflow
{
    public abstract class InspectionRequest
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Name is required")]
        [HtmlContent]
        [StringLength(200, ErrorMessage = "Name exceeds the max length")]
        public string Name { get; set; }
                
        [Required(ErrorMessage = "Frequency is required")]        
        public int? Frequency { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "StartDate is required")]
        [DateAsString(ErrorMessage = "StartDate is not a valid datetime")]
        public string StartDate { get; set; }

        [DateAsString(ErrorMessage = "EndDate is not a valid datetime")]
        [GreaterThan("StartDate", ErrorMessage = "EndDate cannot be before start date")]
        public string EndDate { get; set; }

        [Required(ErrorMessage = "Frequency Unit is required")]
        public SchedulingUnit? FrequencyUnit { get; set; }
        public List<DayOfWeek> FrequencyDaysOfWeek { get; set; }
    }

    public class CreateInspectionRequest : InspectionRequest
    {
        public Guid ZoneId { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "FloorCode is required")]
        [HtmlContent]
        public string FloorCode { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "AssetId is required")]
        public Guid? AssetId { get; set; }
        public string TwinId { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "AssignedWorkgroup is required")]
        public Guid? AssignedWorkgroupId { get; set; }

        [Required(ErrorMessage = "Checks is required")]
        [Unique("Name", ErrorMessage = "Name must be unique")]
        public List<CreateCheckRequest> Checks { get; set; }
    }

    public class UpdateInspectionRequest : InspectionRequest
    {
        public Guid? AssignedWorkgroupId { get; set; }

        [Required(ErrorMessage = "Checks is required")]
        [Unique("Name", ErrorMessage = "Name must be unique")]
        public List<UpdateCheckRequest> Checks { get; set; }
    }


	/// <summary>
	/// This request is to create multi inspections
	/// inspection will be created for each asset in the Asset List
	/// </summary>
	public class CreateInspectionsRequest : InspectionRequest
	{
		/// <summary>
		/// Inspection Zone Id
		/// </summary>
		public Guid ZoneId { get; set; }

		/// <summary>
		/// Asset List for the inspection
		/// </summary>
		[Required(ErrorMessage = "AssetList is required")]
		[MinLength(1, ErrorMessage = "AssetList must have at least one Asset")]
		public List<AssetDto> AssetList { get; set; }

		/// <summary>
		/// Assigned Work Group Id
		/// </summary>
		[Required(AllowEmptyStrings = false, ErrorMessage = "AssignedWorkgroup is required")]
		public Guid? AssignedWorkgroupId { get; set; }

		/// <summary>
		/// Checks required for the inspection
		/// </summary>
		[Required(ErrorMessage = "Checks is required")]
		[Unique("Name", ErrorMessage = "Name must be unique")]
		public List<CreateCheckRequest> Checks { get; set; }
	}
	/// <summary>
	/// Asset details
	/// </summary>
	/// <param name="AssetId"></param>
	/// <param name="FloorCode"></param>
	/// /// <param name="TwinId"></param>
	public record AssetDto
	{
		[Required(AllowEmptyStrings = false, ErrorMessage = "AssetId is required")]
		public Guid? AssetId { get; init; }
		public string TwinId { get; set; }
		[HtmlContent]
		public string FloorCode { get; init; }
	}

}
