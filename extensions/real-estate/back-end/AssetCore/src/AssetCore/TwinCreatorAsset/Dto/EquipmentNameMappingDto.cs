using System;

namespace AssetCore.TwinCreatorAsset.Dto
{
	/// <summary>
	/// Response object that map equipment id  to asset name
	/// </summary>
	public class EquipmentNameMappingDto
	{
		/// <summary>
		/// Equipment Id that should be mapped to asset name
		/// </summary>
		public Guid EquipmentId { get; set; }
		/// <summary>
		/// The asset name mapped to EquipmentId
		/// </summary>
		public string Name { get; set; }
	}
}
