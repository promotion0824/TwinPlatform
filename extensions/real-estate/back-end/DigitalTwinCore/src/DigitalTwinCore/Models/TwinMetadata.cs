using System;
using Azure.DigitalTwins.Core;
using DigitalTwinCore.Dto;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace DigitalTwinCore.Models
{
    [Serializable]
    public class TwinMetadata
    {
        public string ModelId { get; set; }
        public IDictionary<string, TwinPropertyMetadata> WriteableProperties { get; set; }

        internal static TwinMetadata MapFrom(DigitalTwinMetadata dto)
        {
            return new TwinMetadata
            {
                ModelId = dto.ModelId,
                WriteableProperties = dto.PropertyMetadata.ToDictionary(d => d.Key, d => new TwinPropertyMetadata(d.Value.LastUpdatedOn))
            };
        }

        internal static TwinMetadata MapFrom(TwinMetadataDto dto)
        {
            return new TwinMetadata
            {
                ModelId = dto.ModelId,
                WriteableProperties = dto.WriteableProperties.ToDictionary(d => d.Key, d => new TwinPropertyMetadata(d.Value.LastUpdatedOn))
            };
        }
    }

    [Serializable]
    public class TwinPropertyMetadata
    {
        [JsonPropertyName("lastUpdateTime")]
        public DateTimeOffset LastUpdatedOn { get; private set; }

        public TwinPropertyMetadata(DateTimeOffset lastUpdatedOn)
        {
            LastUpdatedOn = lastUpdatedOn;
        }
    }
}
