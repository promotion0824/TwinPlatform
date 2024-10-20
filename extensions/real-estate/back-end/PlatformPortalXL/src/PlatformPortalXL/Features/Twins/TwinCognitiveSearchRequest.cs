using System;

namespace PlatformPortalXL.Features.Twins
{
	public class TwinCognitiveSearchRequest
	{
		/// <summary>
		/// Search term to find on Twin's name, id and externalId
		/// </summary>
		public string Term { get; set; }

		/// <summary>
		/// Twin model id
		/// </summary>
		public string ModelId { get; set; }

		/// <summary>
		/// Site id, if omitted, will search for all sites the user has access to
		/// </summary>
		public Guid[] SiteIds { get; set; } = Array.Empty<Guid>();

		/// <summary>
		/// List of file type extensions
		/// </summary>
		public string[] FileTypes { get; set; }

		/// <summary>
		/// Page requested
		/// </summary>
		public int Page { get; set; } = 1;

		/// <summary>
		/// To export to csv
		/// </summary>
		public bool Export { get; set; }

        /// <summary>
		/// Twin Ids to export to csv
		/// </summary>
		public string[] TwinIds { get; set; } = Array.Empty<string>();

        /// <summary>
		/// To do the sensor search
		/// </summary>
		public bool SensorSearchEnabled { get; set; }

        /// <summary>
        /// If provided, only return twins that belongs to the buildings of the scope.
        /// The scopeId is a dtId of a twin which could be a portfolio, a campus or a site.
        /// </summary>
        public string ScopeId { get; set; }
    }
}
