using System;
using System.Collections.Generic;
using PlatformPortalXL.Auth;

namespace PlatformPortalXL.Features.Twins;

public class TwinSearchResponse
{
    public SearchTwin[] Twins { get; set; }
    public string QueryId { get; set; }
    public int NextPage { get; set; }

    public class SearchTwin : ITwinWithAncestors
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public Guid SiteId { get; set; }
        public string ModelId { get; set; }
        public Guid UniqueId { get; set; }
        public string ExternalId { get; set; }
        public Guid? FloorId { get; set; }
        public string FloorName { get; set; }
        public string RawTwin { get; set; }
        public string FileUrl { get; set; }
        public IEnumerable<SearchRelationship> InRelationships { get; set; }
        public IEnumerable<SearchRelationship> OutRelationships { get; set; }
        public HashSet<string> Locations { get; set; }
        public HashSet<string> ModelIds { get; set; }
        public string TwinId => Id;
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

        /// <summary>The floor name of the related twin</summary>
        public string FloorName { get; set; }
    }
}
