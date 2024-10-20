using System;

namespace DigitalTwinCore.DTO
{
	public class AssetNameDto
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
		public Guid? FloorId { get; set; }
	}
}
