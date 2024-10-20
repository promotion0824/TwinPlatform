using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Linq;

namespace DigitalTwinCore.Features.TwinsSearch.Models
{
    public class SearchTwin
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public Guid SiteId { get; set; }
        public string ModelId { get; set; }
        public Guid UniqueId { get; set; }
        public string ExternalId { get; set; }
        public Guid? FloorId { get; set; }        
        [JsonIgnore]
        public JObject Raw { get; set; }
        public string RawTwin { get; set; }
        public IEnumerable<SearchRelationship> InRelationships { get; set; }
        public IEnumerable<SearchRelationship> OutRelationships { get; set; }
    }

    public class SearchRelationship
    {
        public string SourceId { get; set; }
        public string TargetId { get; set; }

        /// <summary>The name of the relationship, eg. "servedBy", "hasDocument"</summary>
        public string Name { get; set; }

        /// <summary>The model ID of the related twin</summary>
        public string ModelId { get; set; }

        /// <summary>The name of the related twin</summary>
        public string TwinName { get; set; }

        /// <summary>The floor ID of the related twin</summary>
        public Guid? FloorId { get; set; }
    }

    public class SearchTwinCount
    {
        public long Count { get; set; }
    }
}
