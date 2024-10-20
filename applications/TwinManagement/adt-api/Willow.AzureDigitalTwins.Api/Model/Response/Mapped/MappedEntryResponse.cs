using System.Collections.Generic;
using System.Linq;
using Willow.AzureDigitalTwins.Api.Persistence.Models.Mapped;

namespace Willow.AzureDigitalTwins.Api.Model.Response.Mapped
{
    public class MappedEntryResponse
    {
        /// <summary>
        /// Total Count
        /// </summary>
        public long Total { get; set; }

        public IEnumerable<MappedEntry> Items { get; set; } = [];

        public MappedEntryResponse(long total, IEnumerable<MappedEntry> items)
        {
            this.Total = total;
            this.Items = items;
        }
    }
}
