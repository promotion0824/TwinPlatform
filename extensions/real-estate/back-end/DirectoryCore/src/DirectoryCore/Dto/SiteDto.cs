using System;
using System.Collections.Generic;
using System.Linq;
using DirectoryCore.Domain;
using DirectoryCore.Enums;

namespace DirectoryCore.Dto
{
    public class SiteDto
    {
        public Guid Id { get; set; }

        public Guid CustomerId { get; set; }

        public string Name { get; set; }

        public string Code { get; set; }

        public SiteStatus Status { get; set; }

        public SiteFeaturesDto Features { get; set; }

        public string TimezoneId { get; set; }

        public string WebMapId { get; set; }

        public List<ArcGisLayerDto> ArcGisLayers { get; set; }

        public static SiteDto MapFrom(Site site)
        {
            if (site == null)
            {
                return null;
            }

            var siteDto = new SiteDto
            {
                Id = site.Id,
                CustomerId = site.CustomerId,
                Name = site.Name,
                Code = site.Code,
                Status = site.Status,
                Features = SiteFeaturesDto.MapFrom(site.Features),
                ArcGisLayers = ArcGisLayerDto.MapFrom(site.ArcGisLayers),
                TimezoneId = site.TimezoneId,
                WebMapId = site.WebMapId
            };
            return siteDto;
        }

        public static IList<SiteDto> MapFrom(IEnumerable<Site> sites)
        {
            return sites.Select(MapFrom).ToList();
        }
    }
}
