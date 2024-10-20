using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PlatformPortalXL.Dto
{
    /// <summary>
    /// The ids of the entity, these are searched as a whole (keyword)
    /// </summary>
    public class Ids : List<string>
    {
        public Ids(params string[] ids) : base(ids.Where(x => !string.IsNullOrEmpty(x))) { }

        public Ids() : base() { }
    }

    /// <summary>
    /// The location ancestors of the entity, these are searched as a whole (keyword)
    /// </summary>
    public class LocationAncestorIds : List<string>
    {
        public LocationAncestorIds(params string[] ids) : base(ids.Where(x => !string.IsNullOrEmpty(x))) { }

        public LocationAncestorIds() : base() { }
    }

    /// <summary>
    /// The fedby ancestors of the entity, these are searched as a whole (keyword)
    /// </summary>
    public class FedByAncestorIds : List<string>
    {
        public FedByAncestorIds(params string[] ids) : base(ids.Where(x => !string.IsNullOrEmpty(x))) { }

        public FedByAncestorIds() : base() { }
    }

    /// <summary>
    /// The location ancestors of the entity, these are searched as a whole (keyword)
    /// </summary>
    public class FeedsAncestorIds : List<string>
    {
        public FeedsAncestorIds(params string[] ids) : base(ids.Where(x => !string.IsNullOrEmpty(x))) { }

        public FeedsAncestorIds() : base() { }
    }

    /// <summary>
    /// The tenant ancestors of the entity, these are searched as a whole (keyword)
    /// </summary>
    public class TenantAncestorIds : List<string>
    {
        public TenantAncestorIds(params string[] ids) : base(ids.Where(x => !string.IsNullOrEmpty(x))) { }

        public TenantAncestorIds() : base() { }
    }

    /// <summary>
    /// The modelids of the entity, these are searched as a whole (keyword)
    /// </summary>
    public class ModelIds : List<string>
    {
        public ModelIds(params string[] ids) : base(ids.Where(x => !string.IsNullOrEmpty(x))) { }

        public ModelIds() : base() { }
    }


    /// <summary>
    /// The names or descriptions of the entity
    /// </summary>
    public class Names : List<string>
    {
        public Names(params string[] names) : base(names.Where(x => !string.IsNullOrEmpty(x))) { }
        public Names() : base() { }
    }

    /// <summary>
    /// Any other tags for the entity
    /// </summary>
    public class Tags : List<string>
    {
        public Tags(params string[] tags) : base(tags.Where(x => !string.IsNullOrEmpty(x))) { }
        public Tags(IEnumerable<string> tags) : base(tags.Where(x => x is not null)) { }
        public Tags() : base() { }
    }

    /// <summary>
    /// The document we store in Azure Cognitive search and retrieve in response to search queries
    /// </summary>
    public class SearchDocumentDto
    {
        /// <summary>
        /// The key of the entity used by search
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        ///	Primary Id used for the link to the actual page
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Ids of the entity for search, these are searched as a whole (keyword)
        /// </summary>
        public Ids Ids { get; set; }

        /// <summary>
        /// Legacy siteId required by Command
        /// </summary>
        /// <remarks>
        /// Please avoid using this going forward, the twin graph is the source of truth
        /// </remarks>
        public string SiteId { get; set; }

        /// <summary>
        /// ExternalId - refers to the search item in some external system, e.g. timeseries, CMS, ...
        /// </summary>
        public string ExternalId { get; set; }

        /// <summary>
        /// The model ids of the entity, Room, Floor, ... all ancestors
        /// </summary>
        public ModelIds ModelIds { get; set; }

        /// <summary>
        /// Primary model Id for Type=twin, or empty if this is not a twin related search document
        /// </summary>
        public string PrimaryModelId { get; set; }

        /// <summary>
        /// The type of the entity
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Tags for the entity
        /// </summary>
        public Tags Tags { get; set; }

        /// <summary>
        /// The category of the entity
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Importance to boost ranking in search results
        /// </summary>
        public int Importance { get; set; }

        /// <summary>
        /// Any names used for searching
        /// </summary>
        public Names Names { get; set; }

        /// <summary>
        /// Any secondary names or descriptions (lower priority)
        /// </summary>
        public Names SecondaryNames { get; set; }

        /// <summary>
        /// Ancestors by spatial hierarchy
        /// </summary>
        public LocationAncestorIds Location { get; set; }

        /// <summary>
        /// Ancestors that feed this twin/insight/...
        /// </summary>
        public FedByAncestorIds FedBy { get; set; }

        /// <summary>
        /// Ancestors that are fed by this twin/insight/...
        /// </summary>
        public FeedsAncestorIds Feeds { get; set; }

        /// <summary>
        /// Ancestors that are tenant related
        /// </summary>
        public TenantAncestorIds Tenant { get; set; }

        /// <summary>
        /// Some kind of datetime for filtering based on a start date,
        /// e.g. an Insight's earliest occurrence
        /// </summary>
        public DateTimeOffset? Earliest { get; set; }

        /// <summary>
        /// Some kind of datetime for filtering based on an end date,
        /// e.g. an Insight's last occurrence, or a twin last updated date
        /// </summary>
        public DateTimeOffset? Latest { get; set; }


        /// <summary>
        /// Converts the Id to something safe for Azure search key
        /// </summary>
        /// <remarks>
        /// You cannot simply use an objects ID as the key, it needs to be made safe
        /// </remarks>
        private static string SafeKey(string id)
        {
            if (string.IsNullOrEmpty(id)) return "MISSINGID";
            return Convert.ToBase64String(Encoding.ASCII.GetBytes(id))
                // And make it URL safe
                .Replace('+', '-')
                .Replace('/', '_')
                .TrimEnd('=');  // remove padding
        }

        /// <summary>
        /// Creates a new <see cref="SearchDocumentDto" />
        /// </summary>
        public SearchDocumentDto(string type,
            string id,
            Names names,
            Names secondaryNames,
            Ids ids,
            string siteId,
            string externalId,
            LocationAncestorIds locationAncestors,
            FedByAncestorIds fedByAncestors,
            FeedsAncestorIds feedsAncestors,
            TenantAncestorIds tenantAncestors,
            string primaryModelId,
            ModelIds modelids,
            Tags tags,
            string category,
            int importance)
        {
            this.Type = type ?? throw new ArgumentNullException(nameof(type));
            this.Key = SafeKey(type + id);  // unique on ID AND type because insights and rule instances share an ID
            this.Id = id ?? throw new ArgumentNullException(nameof(id));
            this.Ids = ids ?? throw new ArgumentNullException(nameof(ids));
            this.Names = names ?? throw new ArgumentNullException(nameof(names));
            this.SecondaryNames = secondaryNames ?? throw new ArgumentNullException(nameof(secondaryNames));
            this.ModelIds = modelids ?? throw new ArgumentNullException(nameof(modelids));
            this.Tags = tags ?? throw new ArgumentNullException(nameof(tags));
            this.Category = category ?? throw new ArgumentNullException(nameof(category));
            this.Importance = importance;
            this.Location = locationAncestors ?? throw new ArgumentNullException(nameof(locationAncestors));
            this.FedBy = fedByAncestors ?? throw new ArgumentNullException(nameof(fedByAncestors));
            this.Feeds = feedsAncestors ?? throw new ArgumentNullException(nameof(feedsAncestors));
            this.Tenant = tenantAncestors ?? throw new ArgumentNullException(nameof(tenantAncestors));
            this.PrimaryModelId = primaryModelId ?? "";
            this.SiteId = siteId ?? "";
            this.ExternalId = externalId ?? "";
        }

        /// <summary>
        /// Creates a new <see cref="SearchDocumentDto" /> for de-serialization
        /// </summary>
        public SearchDocumentDto()
        {
        }

        public override string ToString()
        {
            return $"{this.Type}: {this.Ids?.First()} - {this.Names?.First()} - {this.Category}";
        }
    }
}
