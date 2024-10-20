namespace Willow.CognitiveSearch;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// The ids of the entity, these are searched as a whole (keyword).
/// </summary>
public class Ids : List<string>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Ids"/> class.
    /// </summary>
    /// <param name="ids">A array of the ids of the entity.</param>
    public Ids(params string[] ids)
        : base(ids.Where(x => !string.IsNullOrEmpty(x))) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Ids"/> class.
    /// </summary>
    public Ids()
        : base() { }
}

/// <summary>
/// The location ancestors of the entity, these are searched as a whole (keyword).
/// </summary>
public class LocationAncestorIds : List<string>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LocationAncestorIds"/> class.
    /// </summary>
    /// <param name="ids">A array of the ids of the entity.</param>
    public LocationAncestorIds(params string[] ids)
        : base(ids.Where(x => !string.IsNullOrEmpty(x))) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LocationAncestorIds"/> class.
    /// </summary>
    public LocationAncestorIds()
        : base() { }
}

/// <summary>
/// The fedby ancestors of the entity, these are searched as a whole (keyword).
/// </summary>
public class FedByAncestorIds : List<string>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FedByAncestorIds"/> class.
    /// </summary>
    /// <param name="ids">A array of the ids of the entity.</param>
    public FedByAncestorIds(params string[] ids)
        : base(ids.Where(x => !string.IsNullOrEmpty(x))) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="FedByAncestorIds"/> class.
    /// </summary>
    public FedByAncestorIds()
        : base() { }
}

/// <summary>
/// The location ancestors of the entity, these are searched as a whole (keyword).
/// </summary>
public class FeedsAncestorIds : List<string>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FeedsAncestorIds"/> class.
    /// </summary>
    /// <param name="ids">A array of the ids of the entity.</param>
    public FeedsAncestorIds(params string[] ids)
        : base(ids.Where(x => !string.IsNullOrEmpty(x))) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="FeedsAncestorIds"/> class.
    /// </summary>
    public FeedsAncestorIds()
        : base() { }
}

/// <summary>
/// The tenant ancestors of the entity, these are searched as a whole (keyword).
/// </summary>
public class TenantAncestorIds : List<string>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TenantAncestorIds"/> class.
    /// </summary>
    /// <param name="ids">A array of the names of the entity.</param>
    public TenantAncestorIds(params string[] ids)
        : base(ids.Where(x => !string.IsNullOrEmpty(x))) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantAncestorIds"/> class.
    /// </summary>
    public TenantAncestorIds()
        : base() { }
}

/// <summary>
/// The modelids of the entity, these are searched as a whole (keyword).
/// </summary>
public class ModelIds : List<string>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ModelIds"/> class.
    /// </summary>
    /// <param name="ids">A array of the ids of the entity.</param>
    public ModelIds(params string[] ids)
        : base(ids.Where(x => !string.IsNullOrEmpty(x))) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModelIds"/> class.
    /// </summary>
    public ModelIds()
        : base() { }
}

/// <summary>
/// The names or descriptions of the entity.
/// </summary>
public class Names : List<string>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Names"/> class.
    /// </summary>
    /// <param name="names">A array of the names of the entity.</param>
    public Names(params string[] names)
        : base(names.Where(x => !string.IsNullOrEmpty(x))) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Names"/> class.
    /// </summary>
    public Names()
        : base() { }
}

/// <summary>
/// The names of location ancestors.
/// </summary>
public class LocationNames : List<string>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LocationNames"/> class.
    /// </summary>
    /// <param name="names">A array of the names of the entity.</param>
    public LocationNames(params string[] names)
        : base(names.Where(x => !string.IsNullOrEmpty(x))) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LocationNames"/> class.
    /// </summary>
    public LocationNames()
        : base() { }
}

/// <summary>
/// Any other tags for the entity.
/// </summary>
public class Tags : List<string>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Tags"/> class.
    /// </summary>
    /// <param name="tags">A array of the tags of the entity.</param>
    public Tags(params string[] tags)
        : base(tags.Where(x => !string.IsNullOrEmpty(x))) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Tags"/> class.
    /// </summary>
    /// <param name="tags">A list of the tags of the entity.</param>
    public Tags(IEnumerable<string> tags)
        : base(tags.Where(x => x is not null)) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Tags"/> class.
    /// </summary>
    public Tags()
        : base() { }
}

/// <summary>
/// The names of models typically used for twin documents.
/// </summary>
public class ModelNames : List<string>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ModelNames"/> class.
    /// </summary>
    /// <param name="modelNames">A array of the model names of the entity.</param>
    public ModelNames(params string[] modelNames)
        : base(modelNames.Where(x => !string.IsNullOrEmpty(x))) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModelNames"/> class.
    /// </summary>
    public ModelNames()
        : base() { }
}

/// <summary>
/// The twin document we store in Azure Cognitive search and retrieve in response to search queries.
/// </summary>
public class UnifiedItemDto
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UnifiedItemDto"/> class.
    /// </summary>
    /// <param name="type">The type of the entity.</param>
    /// <param name="id">The id of the entity.</param>
    /// <param name="names">The names of the entity.</param>
    /// <param name="secondaryNames">The secondary names or descriptions (lower priority).</param>
    /// <param name="ids">The ids of the entity.</param>
    /// <param name="siteId">The legacy siteId required by Command.</param>
    /// <param name="externalId">The external id of the entity.</param>
    /// <param name="locationAncestors">The location ancestors of the entity.</param>
    /// <param name="locationNames">The location names of the entity.</param>
    /// <param name="fedByAncestors">The fedby ancestors of the entity.</param>
    /// <param name="feedsAncestors">The feeds ancestors of the entity.</param>
    /// <param name="tenantAncestors">The tenant ancestors of the entity.</param>
    /// <param name="primaryModelId">Primary model Id for Type=twin, or empty if this is not a twin related search document.</param>
    /// <param name="modelids">The model ids of the entity.</param>
    /// <param name="modelNames">The model names of the entity.</param>
    /// <param name="tags">Tags for the entity.</param>
    /// <param name="category">The category of the entity.</param>
    /// <param name="importance">Importance to boost ranking in search results.</param>
    public UnifiedItemDto(
        string type,
        string id,
        Names names,
        Names secondaryNames,
        Ids ids,
        string siteId,
        string externalId,
        LocationAncestorIds locationAncestors,
        LocationNames locationNames,
        FedByAncestorIds fedByAncestors,
        FeedsAncestorIds feedsAncestors,
        TenantAncestorIds tenantAncestors,
        string primaryModelId,
        ModelIds modelids,
        ModelNames modelNames,
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
        this.ModelNames = modelNames ?? throw new ArgumentNullException(nameof(modelNames));
        this.Tags = tags ?? throw new ArgumentNullException(nameof(tags));
        this.Category = category ?? throw new ArgumentNullException(nameof(category));
        this.Importance = importance;
        this.Location = locationAncestors ?? throw new ArgumentNullException(nameof(locationAncestors));
        this.LocationNames = locationNames ?? throw new ArgumentNullException(nameof(locationNames));
        this.FedBy = fedByAncestors ?? throw new ArgumentNullException(nameof(fedByAncestors));
        this.Feeds = feedsAncestors ?? throw new ArgumentNullException(nameof(feedsAncestors));
        this.Tenant = tenantAncestors ?? throw new ArgumentNullException(nameof(tenantAncestors));
        this.PrimaryModelId = primaryModelId ?? string.Empty;
        this.SiteId = siteId ?? string.Empty;
        this.ExternalId = externalId ?? string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UnifiedItemDto"/> class.
    /// </summary>
#pragma warning disable CS8618
    public UnifiedItemDto()
    {
    }
#pragma warning restore CS8618

    /// <summary>
    /// Gets or sets the key of the entity used by search.
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    /// Gets or sets primary Id used for the link to the actual page.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Gets or sets ids of the entity for search, these are searched as a whole (keyword).
    /// </summary>
    public Ids Ids { get; set; }

    /// <summary>
    /// Gets or sets legacy siteId required by Command.
    /// </summary>
    /// <remarks>
    /// Please avoid using this going forward, the twin graph is the source of truth.
    /// </remarks>
    public string SiteId { get; set; }

    /// <summary>
    /// Gets or sets externalId - refers to the search item in some external system, e.g. timeseries, CMS, ...
    /// </summary>
    public string ExternalId { get; set; }

    /// <summary>
    /// Gets or sets the model ids of the entity, Room, Floor, ... all ancestors.
    /// </summary>
    public ModelIds ModelIds { get; set; }

    /// <summary>
    /// Gets or sets the model names of the entity, Room, Floor, ... all ancestors.
    /// </summary>
    public ModelNames ModelNames { get; set; }

    /// <summary>
    /// Gets or sets primary model Id for Type=twin, or empty if this is not a twin related search document.
    /// </summary>
    public string PrimaryModelId { get; set; }

    /// <summary>
    /// Gets or sets the type of the entity.
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// Gets or sets tags for the entity.
    /// </summary>
    public Tags Tags { get; set; }

    /// <summary>
    /// Gets or sets the category of the entity.
    /// </summary>
    public string Category { get; set; }

    /// <summary>
    /// Gets or sets importance to boost ranking in search results.
    /// </summary>
    public int Importance { get; set; }

    /// <summary>
    /// Gets or sets any names used for searching.
    /// </summary>
    public Names Names { get; set; }

    /// <summary>
    /// Gets or sets any secondary names or descriptions (lower priority).
    /// </summary>
    public Names SecondaryNames { get; set; }

    /// <summary>
    /// Gets or sets ancestors by spatial hierarchy.
    /// </summary>
    public LocationAncestorIds Location { get; set; }

    /// <summary>
    /// Gets or sets ancestor names by spatial hierarchy.
    /// </summary>
    public LocationNames LocationNames { get; set; }

    /// <summary>
    /// Gets or sets ancestors that feed this twin/insight/...
    /// </summary>
    public FedByAncestorIds FedBy { get; set; }

    /// <summary>
    /// Gets or sets ancestors that are fed by this twin/insight/...
    /// </summary>
    public FeedsAncestorIds Feeds { get; set; }

    /// <summary>
    /// Gets or sets ancestors that are tenant related.
    /// </summary>
    public TenantAncestorIds Tenant { get; set; }

    /// <summary>
    /// Gets or sets some kind of datetime for filtering based on a start date,
    /// e.g. an Insight's earliest occurrence.
    /// </summary>
    public DateTimeOffset? Earliest { get; set; }

    /// <summary>
    /// Gets or sets some kind of datetime for filtering based on an end date,
    /// e.g. an Insight's last occurrence, or a twin last updated date.
    /// </summary>
    public DateTimeOffset? Latest { get; set; }

    /// <summary>
    /// Gets or sets the date and time the document was indexed.
    /// </summary>
    public DateTimeOffset? IndexedDate { get; set; }

    /// <summary>
    /// Converts the Id to something safe for Azure search key.
    /// </summary>
    /// <remarks>
    /// You cannot simply use an objects ID as the key, it needs to be made safe.
    /// </remarks>
    private static string SafeKey(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return "MISSINGID";
        }

        return Convert.ToBase64String(Encoding.ASCII.GetBytes(id))
            .Replace('+', '-') // And make it URL safe
            .Replace('/', '_')
            .TrimEnd('=');  // remove padding
    }

    /// <summary>
    /// Returns a short summary of the SearchDocumentDto.
    /// </summary>
    /// <returns>The resulting string.</returns>
    public override string ToString()
    {
        return $"{this.Type}: {this.Ids?.First()} - {this.Names?.First()} - {this.Category}";
    }
}
