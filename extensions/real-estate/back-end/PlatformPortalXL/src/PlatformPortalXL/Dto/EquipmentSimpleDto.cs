using PlatformPortalXL.Models;
using PlatformPortalXL.ServicesApi.DigitalTwinApi;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PlatformPortalXL.Dto
{
	public class EquipmentSimpleDto
	{
		public Guid EquipmentId { get; set; }
		public string Name { get; set; }


		static EquipmentSimpleDto MapFromResponse(TwinSimpleResponse response)
		{
			var dto = new EquipmentSimpleDto();
			if (response == null)
			{
				return dto;
			}
			dto.EquipmentId = response.UniqueId;
			dto.Name = response.Name;
			return dto;

		}

		public static List<EquipmentSimpleDto> MapFromResponseList(IEnumerable<TwinSimpleResponse> responses)
		{
			if(responses == null || !responses.Any())
			{
				return new List<EquipmentSimpleDto>();
			}
			return responses.Select(x => MapFromResponse(x)).ToList();
		}
	}

	
}
