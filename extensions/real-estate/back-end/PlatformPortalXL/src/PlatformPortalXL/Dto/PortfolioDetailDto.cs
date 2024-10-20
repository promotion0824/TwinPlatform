using System;
using System.Collections.Generic;
using System.Linq;
using PlatformPortalXL.Models;

using Willow.Management;
using Willow.Platform.Models;

namespace PlatformPortalXL.Dto
{
    public class PortfolioDetailDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public PortfolioFeaturesDto Features { get; set; }
        public List<SiteDto> Sites { get; set; }

        public class SiteDto
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
        }

        public static PortfolioDetailDto MapFrom(Portfolio portfolio, IEnumerable<Site> sites)
        {
            return new PortfolioDetailDto
            {
                Id = portfolio.Id,
                Name = portfolio.Name,
                Features = PortfolioFeaturesDto.MapFrom(portfolio.Features),
                Sites = sites.Select(x => new SiteDto {Id = x.Id, Name = x.Name}).ToList()
            };
        }
    }
}
