using System;
using System.Collections.Generic;

namespace DigitalTwinCore.DTO
{
	public class LightCategoryDto
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
		public string ModelId { get; set; }
		public IEnumerable<LightCategoryDto> ChildCategories { get; set; }
		public long AssetCount { get; set; }
		public bool HasChildren { get; set; }
	}
}
