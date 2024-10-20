using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Azure.DigitalTwins.Core;
using Willow.AzureDigitalTwins.Api.Extensions;

namespace Willow.AzureDigitalTwins.Api.Custom
{
    [Serializable]
    public class RealEstateTwin
    {
        public string Id { get; set; }
        public RealEstateTwinMetadata Metadata { get; set; }
        public string Etag { get; set; }
        public string ModelId { get; set; }
        public IDictionary<string, object> CustomProperties { get; set; }

        public const string TwinPropertyMetaData = "TwinPropertyMetaData";

        internal static RealEstateTwin MapFrom(BasicDigitalTwin dto)
        {
            return new RealEstateTwin
            {
                CustomProperties = dto.Contents.AsEnumerable().ToDictionary(
                    kv => kv.Key,
                    kv => kv.Value switch
                    {
                        JsonElement element => element.ToObject(),
                        _ => kv.Value
                    }),
                Id = dto.Id,
                ModelId = dto.Metadata.ModelId,
                Metadata = new RealEstateTwinMetadata { ModelId = dto.Metadata.ModelId, LastUpdatedOn = dto.LastUpdatedOn }
            };
        }

        public BasicDigitalTwin ToBasicDigitalTwin()
        {
            return new BasicDigitalTwin
            {
                Contents = CustomProperties,
                Id = Id,
                Metadata = new DigitalTwinMetadata
                {
                    ModelId = Metadata != null ? Metadata.ModelId : ModelId,
                    PropertyMetadata = new Dictionary<string, DigitalTwinPropertyMetadata>()
                    {{TwinPropertyMetaData, Metadata.LastUpdatedOn.HasValue?
                         new DigitalTwinPropertyMetadata()
                            { LastUpdatedOn = Metadata.LastUpdatedOn.Value}:
                         new DigitalTwinPropertyMetadata()
                     }
                    }
                }
            };
        }
    }

    [Serializable]
    public class RealEstateTwinMetadata
    {
        public string ModelId { get; set; }
        public DateTimeOffset? LastUpdatedOn { get; set; }
    }
}
