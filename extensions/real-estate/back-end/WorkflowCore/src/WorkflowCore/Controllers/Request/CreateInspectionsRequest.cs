using System.Collections.Generic;
using System;

namespace WorkflowCore.Controllers.Request
{
	/// <summary>
	/// This request is to create multi inspections
	/// inspection will be created for each asset in the Asset List
	/// </summary>
	public class CreateInspectionsRequest : InspectionRequest
	{
		/// <summary>
		/// Zone Id
		/// </summary>
		public Guid ZoneId { get; set; }
		/// <summary>
		/// Asset List for the inspection
		/// </summary>
		public List<AssetDto> AssetList { get; set; }
		/// <summary>
		/// Checks required for the inspection
		/// </summary>
		public override List<CheckRequest> Checks { get; set; }
	}
	/// <summary>
	/// Asset details
	/// </summary>
	/// <param name="AssetId"></param>
	/// <param name="FloorCode"></param>
	public class AssetDto
	{
		[Obsolete("This field is no loner in use")]
		public Guid AssetId { get; set; }
		public string TwinId { get; set; }
		public string FloorCode { get; set; }
	}
}
