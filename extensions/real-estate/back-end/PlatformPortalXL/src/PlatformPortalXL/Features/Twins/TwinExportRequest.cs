using System;
using System.Linq;
using Willow.Common;

namespace PlatformPortalXL.Features.Twins
{
    public class TwinExportRequest
    {
        public string QueryId { get; set; }
        public TwinExport[] Twins { get; set; }

        public void Validate()
        {
            if (Twins == null)
            {
                Twins = Array.Empty<TwinExport>();
            }
            else if (!Twins.Any())
            {
                throw new ArgumentException("Twins cannot be empty.").WithData(this);
            }
        }
    }

    public class TwinExport
    {
        public Guid SiteId { get; set; }
        public string TwinId { get; set; }
    }

}
