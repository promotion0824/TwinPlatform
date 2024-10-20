using System;
using System.Collections.Generic;
using System.Linq;
using DirectoryCore.Domain;

namespace DirectoryCore.Dto
{
    public class SiteMiniDto
    {
        public Guid Id { get; set; }
        public Guid? PortfolioId { get; set; }
        public string Name { get; set; }
        public string Suburb { get; set; }
        public string State { get; set; }
        public Guid? LogoId { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string Status { get; set; }
        public string Type { get; set; }

        public static SiteMiniDto MapFrom(Site site)
        {
            if (site == null)
            {
                return null;
            }

            var siteDto = new SiteMiniDto
            {
                Id = site.Id,
                PortfolioId = site.PortfolioId,
                Name = site.Name,
                Suburb = site.Suburb,
                State = site.State,
                LogoId = site.LogoId,
                Latitude = site.Latitude,
                Longitude = site.Longitude,
                Status = site.Status.ToString(),
                Type = site.Type
            };
            return siteDto;
        }

        public static IList<SiteMiniDto> MapFrom(IEnumerable<Site> sites)
        {
            return sites.Select(MapFrom).ToList();
        }
    }
}
