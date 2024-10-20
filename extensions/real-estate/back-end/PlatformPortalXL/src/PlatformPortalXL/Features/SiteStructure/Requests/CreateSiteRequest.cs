using System;
using PlatformPortalXL.Attributes;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Willow.Platform.Models;

namespace PlatformPortalXL.Features.SiteStructure.Requests
{
    public class CreateSiteRequest
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Code { get; set; }
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
        [ListCannotBeEmpty]
        public List<string> FloorCodes { get; set; }
        public SiteFeatures Features { get; set; }
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
        public DateOnly? DateOpened { get; set; }
    }
}
