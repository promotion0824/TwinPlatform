using DigitalTwinCore.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DigitalTwinCore.Dto
{
    public class RelationshipDto
    {
        public string Id { get; set; }
        public string TargetId { get; set; }
        public string SourceId { get; set; }
        public string Name { get; set; }
        public TwinDto Target { get; set; }
        public TwinDto Source { get; set; }
        public string Substance { get; set; }

        [JsonExtensionData]
        public IDictionary<string, object> CustomProperties { get; set; } = new Dictionary<string, object>();

        public static IDictionary<string, object> MapCustomProperties(IDictionary<string, object> customProperties)
        {
            if (customProperties == null) return null;

            return customProperties.AsEnumerable().ToDictionary(
                kv => kv.Key,
                kv => kv.Value switch
                {
                    JsonElement element => element.ToObject(),
                    _ => kv.Value
                });
        }

        public static RelationshipDto MapFrom(TwinRelationship model)
        {
            return new RelationshipDto
            {
                Id = model.Id,
                Name = model.Name,
                SourceId = model.Source.Id,
                TargetId = model.Target.Id,
                CustomProperties = MapCustomProperties(model.CustomProperties),
                Target = TwinDto.MapFrom(model.Target),
                Source = TwinDto.MapFrom(model.Source),
                Substance = model.CustomProperties?.GetStringValueOrDefault("substance")
            };
        }

        public static List<RelationshipDto> MapFrom(IEnumerable<TwinRelationship> models)
        {
            if (models == null)
            {
                return new List<RelationshipDto>();
            }
            return models.Select(MapFrom).ToList();
        }

        public static RelationshipDto MapFrom(Relationship model)
        {
            return new RelationshipDto
            {
                Id = model.Id,
                Name = model.Name,
                SourceId = model.SourceId,
                TargetId = model.TargetId,
                CustomProperties = model.CustomProperties
            };
        }

        public static List<RelationshipDto> MapFrom(IEnumerable<Relationship> models)
        {
            if (models == null)
            {
                return new List<RelationshipDto>();
            }
            return models.Select(MapFrom).ToList();
        }
    }
}
