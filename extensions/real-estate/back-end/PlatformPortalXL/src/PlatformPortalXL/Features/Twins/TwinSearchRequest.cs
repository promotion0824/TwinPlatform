using System;

namespace PlatformPortalXL.Features.Twins
{
    public class TwinSearchRequest
    {
        /// <summary>
        /// Search term to find on Twin's name
        /// </summary>
        public string Term { get; set; }

        /// <summary>
        /// List of file type extensions
        /// </summary>
        public string[] FileTypes { get; set; }

        /// <summary>
        /// Twin model id
        /// </summary>
        public string ModelId { get; set; }

        /// <summary>
        /// Site id, if omitted, will search for all sites the user has access to
        /// </summary>
        public Guid[] SiteIds { get; set; } = Array.Empty<Guid>();

        /// <summary>
        /// Persisted query id for pagination retrieval
        /// </summary>
        public string QueryId { get; set; }

        /// <summary>
        /// Page requested, QueryId is required
        /// </summary>
        public int Page { get; set; } = 1;

        /// <summary>
        /// If provided, only return twins that are the source of an isCapabilityOf
        /// relationship, the target of which matches this model ID.
        /// </summary>
        public string IsCapabilityOfModelId { get; set; }

        /// <summary>
        /// If provided, only return twins that belongs to the buildings of the scope.
        /// The scopeId is a dtId of a twin which could be a portfolio, a campus or a site.
        /// </summary>
        public string ScopeId { get; set; }
    }
}
