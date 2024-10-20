using System;
using System.Collections.Generic;
using Willow.Platform.Models;

namespace Willow.Management
{
    public class ManagedPortfolioDto
    {
        public Guid         PortfolioId { get; set; }
        public string       PortfolioName { get; set; }
        public PortfolioFeaturesDto Features { get; set; }
        public string Role { get; set; }
        public List<ManagedSiteDto> Sites { get; set; }
    }

    public class ManagedSiteDto
    {
        public Guid SiteId { get; set; }
        public string SiteName { get; set; }
        public string Role { get; set; }
        public string LogoUrl { get; set; }
        public string LogoOriginalSizeUrl { get; set; }
    }
}
