using System;
using System.Collections.Generic;
using System.Linq;
using PlatformPortalXL.Models;

using Willow.Management;
using Willow.Platform.Models;

namespace PlatformPortalXL.Dto
{
    public class PortfolioDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public PortfolioFeaturesDto Features { get; set; }
        public int? SiteCount { get; set; }

        public static PortfolioDto MapFrom(Portfolio model)
        {
            return new PortfolioDto
            {
                Id = model.Id,
                Name = model.Name,
                Features = PortfolioFeaturesDto.MapFrom(model.Features),
                SiteCount = model.Sites?.Count
            };
        }

        public static List<PortfolioDto> MapFrom(IEnumerable<Portfolio> models)
        {
            return models?.Select(MapFrom).ToList();
        }
    }
}
