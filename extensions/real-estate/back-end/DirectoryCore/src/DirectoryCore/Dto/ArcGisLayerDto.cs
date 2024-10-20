using System.Collections.Generic;
using System.Linq;
using DirectoryCore.Domain;

namespace DirectoryCore.Dto
{
    public class ArcGisLayerDto
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public string Url { get; set; }

        public static ArcGisLayerDto MapFrom(ArcGisLayer model)
        {
            return new ArcGisLayerDto
            {
                Id = model.Id,
                Title = model.Title,
                Type = model.Type,
                Url = model.Url
            };
        }

        public static List<ArcGisLayerDto> MapFrom(IEnumerable<ArcGisLayer> models)
        {
            return models?.Select(MapFrom).ToList();
        }
    }
}
