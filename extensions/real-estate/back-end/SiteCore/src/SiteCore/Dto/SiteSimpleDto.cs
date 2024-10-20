using System;
using System.Collections.Generic;
using System.Linq;
using SiteCore.Domain;
using SiteCore.Services.ImageHub;

namespace SiteCore.Dto
{
    public class SiteSimpleDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public Guid? PortfolioId { get; set; }
        public string TimeZoneId { get; set; }
        public Guid? LogoId { get; set; }
        public string LogoPath { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }        
        public DateTime? CreatedDate { get; set; }
        public Guid CustomerId { get; set; }

        public static SiteSimpleDto MapFrom(Site site, IImagePathHelper helper)
        {
            if (site == null)
            {
                return null;
            }

            return new SiteSimpleDto
            {
                Id = site.Id,
                Code = site.Code,
                Name = site.Name,
                PortfolioId = site.PortfolioId,
                TimeZoneId = site.Timezone.Id,
                LogoId = site.LogoId,
                LogoPath = helper.GetSiteLogoPath(site.CustomerId, site.Id),
                Latitude = site.Latitude,
                Longitude = site.Longitude,
                CreatedDate = site.CreatedDate,
                CustomerId = site.CustomerId
            };
        }

        public static List<SiteSimpleDto> MapFrom(IEnumerable<Site> sites, IImagePathHelper helper)
        {
            return sites?.Select(x => MapFrom(x, helper)).ToList();
        }
    }
}
