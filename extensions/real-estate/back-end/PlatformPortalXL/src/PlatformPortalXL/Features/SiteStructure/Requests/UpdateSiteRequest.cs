using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Willow.Platform.Models;
using Willow.Workflow;

namespace PlatformPortalXL.Features.SiteStructure.Requests
{
    public class UpdateSiteRequest
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Address { get; set; }
        [Required]
        public string Suburb { get; set; }
        [Required]
        public string Country { get; set; }
        [Required]
        public string State { get; set; }
        [Required]
        public string TimeZoneId { get; set; }
        [Required]
        public double Latitude { get; set; }
        [Required]
        public double Longitude { get; set; }
        public SiteFeatures Features { get; set; }
        public SiteSettings Settings { get; set; }
        [Required]
        public string Area { get; set; }
        [Required]
        public PropertyType? Type { get; set; }
        [Required]
        public SiteStatus? Status { get; set; }
        public int? ConstructionYear { get; set; }
        public string SiteCode { get; set; }
        public string SiteContactName { get; set; }
        public string SiteContactEmail { get; set; }
        public string SiteContactTitle { get; set; }
        public string SiteContactPhone { get; set; }
		public List<ArcGisLayer> ArcGisLayers { get; set; }
		public string WebMapId { get; set; }
        public DateOnly? DateOpened { get; set; }
	}
}
