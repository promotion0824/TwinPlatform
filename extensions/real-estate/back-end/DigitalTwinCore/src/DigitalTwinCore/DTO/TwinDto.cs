using DigitalTwinCore.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Azure;

namespace DigitalTwinCore.Dto
{
    public class TwinDto
    {
        public string Id { get; set; }
        public TwinMetadataDto Metadata { get; set; }
        public string UserId { get; set; }
        public string Etag { get; set; }

        [JsonExtensionData]
        public IDictionary<string, object> CustomProperties { get; set; }

        public TwinDto[] Children { get; set; }

        public static TwinDto MapFrom(Twin model)
        {
            if (model == null) return null;
            return new TwinDto
            {
                CustomProperties = model.CustomProperties,
                Id = model.Id,
                Metadata = model.Metadata != null ? TwinMetadataDto.MapFrom(model.Metadata) : null,
                Etag = model.Etag
            };
        }

        internal static IEnumerable<TwinDto> MapFrom(IEnumerable<Twin> models)
        {
            return models.Select(MapFrom);
        }
    }

    public class TwinIdDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string UniqueId { get; set; }

        public static TwinIdDto MapFrom(Twin model)
        {
            return new TwinIdDto
            {
                Id = model.Id,
                UniqueId = model.UniqueId.ToString(),
                Name = model.GetStringProperty(Constants.Properties.Name)
            };
        }

        internal static IEnumerable<TwinIdDto> MapFrom(IEnumerable<Twin> models)
        {
            return models.Select(MapFrom);
        }
    }

    public class TwinRealestateIdDto : TwinIdDto
    {
        public string GeometryViewerId { get; set; }

        public static TwinRealestateIdDto IdsMapFrom(Twin model)
        {
            return new TwinRealestateIdDto
            {
                Id = model.Id,
                UniqueId = model.UniqueId.ToString(),
                GeometryViewerId = model.GetStringProperty(Constants.Properties.GeometryViewerId)
            };
        }

        internal static IEnumerable<TwinRealestateIdDto> IdsMapFrom(IEnumerable<Twin> models)
        {
            return models.Select(IdsMapFrom);
        }
    }
}
