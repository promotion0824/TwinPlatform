using System;

using Willow.Platform.Models;

namespace PlatformPortalXL.ServicesApi.DirectoryApi
{
    public class DirectoryApiCreateSiteRequest
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public SiteFeatures Features { get; set; }
        public string TimeZoneId { get; set; }
    }
}
