using System;
using System.Collections.Generic;
using System.Linq;
using PlatformPortalXL.Services;
using Willow.Platform.Models;

namespace PlatformPortalXL.Dto
{
    public class SiteMiniDto
    {
        public Guid Id { get; set; }
        public Guid? PortfolioId { get; set; }
        public string Name { get; set; }
        public string Suburb { get; set; }
        public string State { get; set; }
        public string LogoUrl { get; set; }
        public string LogoOriginalSizeUrl { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string Status { get; set; }
        public string Type { get; set; }
        public WeatherDto Weather { get; set; }

        public static SiteMiniDto Map(Site model, IImageUrlHelper helper)
        {
            return new SiteMiniDto
            {
                Id = model.Id,
                PortfolioId = model.PortfolioId,
                Name = model.Name,
                Suburb = model.Suburb,
                State = model.State,
                Latitude = model.Latitude,
                Longitude = model.Longitude,
                Status = model.Status.ToString(),
                Type = model.Type.ToString(),
                LogoUrl = model.LogoId.HasValue ? helper.GetSiteLogoUrl(model.LogoPath, model.LogoId.Value) : null,
                LogoOriginalSizeUrl = model.LogoId.HasValue ? helper.GetSiteLogoOriginalSizeUrl(model.LogoPath, model.LogoId.Value) : null
            };
        }

        public static List<SiteMiniDto> Map(IEnumerable<Site> models, IImageUrlHelper helper)
        {
            return models?.Select(x => Map(x, helper)).ToList();
        }
    }
}
