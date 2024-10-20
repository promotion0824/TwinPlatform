using System;
using System.Collections.Generic;
using System.Linq;
using DirectoryCore.Domain;

namespace DirectoryCore.Dto
{
    public class PortfolioDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public PortfolioFeaturesDto Features { get; set; }
        public List<SiteDto> Sites { get; set; }

        public static PortfolioDto MapFrom(Portfolio portfolio)
        {
            if (portfolio == null)
            {
                return null;
            }

            var portfolioDto = new PortfolioDto
            {
                Id = portfolio.Id,
                Name = portfolio.Name,
                Features = PortfolioFeaturesDto.MapFrom(portfolio.Features),
                Sites = portfolio.Sites == null ? null : SiteDto.MapFrom(portfolio.Sites).ToList()
            };
            return portfolioDto;
        }

        public static List<PortfolioDto> MapFrom(IEnumerable<Portfolio> portfolios)
        {
            return portfolios?.Select(MapFrom).ToList();
        }
    }
}
