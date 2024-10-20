using DigitalTwinCore.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace DigitalTwinCore.Dto
{
    public class TwinAdxDto
    {
        [JsonExtensionData]
        public IDictionary<string, object> CustomProperties { get; set; }

        public string Raw { get; set; }

        public string Id { get; set; }

        public TwinMetadataDto Metadata { get; set; }

        public static TwinAdxDto MapFrom(TwinAdx model)
        {
            return new TwinAdxDto
            {
                CustomProperties = model.CustomProperties,
                Id = model.Id,
                Metadata = TwinMetadataDto.MapFrom(model.Metadata),
                Raw = model.Raw
            };
        }

        internal static IEnumerable<TwinAdxDto> MapFrom(IEnumerable<TwinAdx> models)
        {
            return models.Select(MapFrom);
        }
    }
}
