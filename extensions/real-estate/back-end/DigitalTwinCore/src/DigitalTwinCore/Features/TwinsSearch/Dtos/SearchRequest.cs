using System;

namespace DigitalTwinCore.Features.TwinsSearch.Dtos
{
    public class SearchRequest
    {
        /// <summary>
        /// List of site ids to search
        /// </summary>
        public Guid[] SiteIds { get; set; }

        /// <summary>
        /// Term to find on twin's name
        /// </summary>
        public string Term { get; set; }

        /// <summary>
        /// File Types
        /// </summary>
        public string[] FileTypes { get; set; }

        /// <summary>
        /// Twin category
        /// UniqueId generated from the DTDL modelId. e.g. fe8f5743-a382-7ffa-9213-70e5e0d493f6
        /// </summary>
        public Guid? CategoryId { get; set; }

        /// <summary>
        /// Twin modelId
        /// Id of DTDL model. e.g. dtmi:com:willowinc:Asset;1
        /// </summary>
        public string ModelId { get; set; }

        /// <summary>
        /// Stored query on ADX for paging
        /// </summary>
        public string QueryId { get; set; }

        /// <summary>
        /// Page number, it only has effect on stored queries
        /// </summary>
        public int Page { get; set; } = 0;

        /// <summary>
        /// Size of the page
        /// </summary>
        public int PageSize { get; set; } = 100;

        /// <summary>
        /// If provided, only return twins that are the source of an isCapabilityOf
        /// relationship, the target of which matches this model ID.
        /// </summary>
        public string IsCapabilityOfModelId { get; set; }
    }
}
