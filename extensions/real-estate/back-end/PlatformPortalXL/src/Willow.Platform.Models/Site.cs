using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Willow.Platform.Models
{
    [DebuggerDisplay("{Code} - {Name}")]
    public class Site
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public Guid? PortfolioId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Suburb { get; set; }
        public string Address { get; set; }
        public string State { get; set; }
        public string Postcode { get; set; }
        public string Country { get; set; }
        public int NumberOfFloors { get; set; }
        public Guid? LogoId { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string TimeZoneId { get; set; }
        public string Area { get; set; }
        public PropertyType Type { get; set; }
        public SiteStatus Status { get; set; }
        public int? ConstructionYear { get; set; }
        public string SiteCode { get; set; }
        public string SiteContactName { get; set; }
        public string SiteContactEmail { get; set; }
        public string SiteContactTitle { get; set; }
        public string SiteContactPhone { get; set; }
        public string LogoPath { get; set; }
        public string WebMapId { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateOnly? DateOpened { get; set; }

        public SiteFeatures Features { get; set; }

        public List<Widget> Widgets { get; set; } = [];
		public List<ArcGisLayer> ArcGisLayers { get; set; }
	}
}
