
using PlatformPortalXL.Models;
using System.Collections.Generic;
using Willow.Platform.Models;

namespace PlatformPortalXL.ServicesApi.DirectoryApi
{
    public class DirectoryApiUpdateSiteRequest
    {
        public string Name { get; set; }
        public SiteFeatures Features { get; set; }
        public string TimeZoneId { get; set; }
        public SiteStatus Status { get; set; }
		public List<ArcGisLayer> ArcGisLayers { get; set; }
        public string WebMapId { get; set; }
	}
}
