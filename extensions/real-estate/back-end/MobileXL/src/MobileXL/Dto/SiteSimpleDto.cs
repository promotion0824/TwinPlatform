using System;
using System.Collections.Generic;
using System.Linq;
using MobileXL.Models;

namespace MobileXL.Dto
{
    public class SiteSimpleDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string TimeZone { get; set; }
        public string AccountExternalId { get; set; }
		public Guid CustomerId { get; set; }
		public SiteFeaturesDto Features { get; set; }

        public static SiteSimpleDto MapFrom(Site site)
        {
            if (site == null)
            {
                return null;
            }

            return new SiteSimpleDto
            {
                Id = site.Id,
                Name = site.Name,
                Address = site.Address,
				CustomerId = site.CustomerId,
				Features = SiteFeaturesDto.Map(site.Features)
            };

        }

        public static List<SiteSimpleDto> MapFrom(IEnumerable<Site> sites)
        {
            return sites?.Select(MapFrom).ToList();
        }
    }
}
