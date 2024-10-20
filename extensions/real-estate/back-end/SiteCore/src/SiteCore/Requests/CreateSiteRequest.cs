using SiteCore.Domain;
using SiteCore.Enums;
using System;
using System.Collections.Generic;

namespace SiteCore.Requests
{
    public class CreateSiteRequest
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public string Address { get; set; }
        public string Suburb { get; set; }
        public string Country { get; set; }
        public string State { get; set; }
        public string TimeZoneId { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public SiteStatus Status { get; set; }
        public PropertyType Type { get; set; }
        public string Area { get; set; }
        public List<string> FloorCodes { get; set; }

        public int? ConstructionYear { get; set; }
        public string SiteCode { get; set; }
        public string SiteContactName { get; set; }
        public string SiteContactEmail { get; set; }
        public string SiteContactTitle { get; set; }
        public string SiteContactPhone { get; set; }
		public Guid Id { get; set; }
		public SiteFeatures Features { get; set; }
        public DateOnly? DateOpened { get; set; }
	}
}
