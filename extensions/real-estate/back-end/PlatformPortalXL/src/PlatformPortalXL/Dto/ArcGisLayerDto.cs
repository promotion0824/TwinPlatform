using PlatformPortalXL.Features.Management;
using static Microsoft.Azure.Amqp.Serialization.SerializableType;
using System.Collections.Generic;
using Willow.Platform.Users;
using Willow.Workflow;
using Willow.Platform.Models;
using System.Linq;

namespace PlatformPortalXL.Dto
{
	public class ArcGisLayerDto
	{
		public string Id { get; set; }
		public string Title { get; set; }
		public string Type { get; set; }
		public string Url { get; set; }

		public static ArcGisLayerDto Map(ArcGisLayer model)
		{
            if (model is null)
            {
                return null;
            }
            return new ArcGisLayerDto
			{
				Id = model.Id,
				Title = model.Title,
				Type = model.Type,
				Url = model.Url
			};
		}

		public static List<ArcGisLayerDto> Map(IEnumerable<ArcGisLayer> models)
		{
			return models?.Select(Map).ToList();
		}
	}
}
