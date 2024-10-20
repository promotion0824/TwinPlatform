using Azure.DigitalTwins.Core;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using DigitalTwinCore.Models;
using DigitalTwinCore.Infrastructure.Json;

namespace DigitalTwinCore.Dto
{
    [JsonConverter(typeof(TwinMetadataJsonConverter))]
    public class TwinMetadataDto
    {
        public string ModelId { get; set; }

        [JsonExtensionData]
        public IDictionary<string, TwinPropertyMetadata> WriteableProperties { get; set; } = new Dictionary<string, TwinPropertyMetadata>();

        internal static TwinMetadataDto MapFrom(TwinMetadata model)
        {
            return new TwinMetadataDto
            {
                ModelId = model.ModelId,
                WriteableProperties = model.WriteableProperties
            };
        }
    }
}
