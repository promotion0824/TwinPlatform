using System;
using System.Collections.Generic;
using DirectoryCore.Domain;
using DirectoryCore.Enums;

namespace DirectoryCore.Controllers.Requests
{
    public abstract class BaseSiteRequest
    {
        public string Name { get; set; }
        public SiteFeatures Features { get; set; }
        public string TimeZoneId { get; set; }
        public List<ArcGisLayer> ArcGisLayers { get; set; }
        public string WebMapId { get; set; }
    }

    public class CreateSiteRequest : BaseSiteRequest
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
    }

    public class UpdateSiteRequest : BaseSiteRequest
    {
        public SiteStatus Status { get; set; }
    }
}
