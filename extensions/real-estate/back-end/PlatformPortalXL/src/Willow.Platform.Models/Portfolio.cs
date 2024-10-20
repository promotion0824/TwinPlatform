using System;
using System.Collections.Generic;

namespace Willow.Platform.Models
{
    public class Portfolio
    {
        public Guid              Id         { get; set; }
        public string            Name       { get; set; }
        public PortfolioFeatures Features   { get; set; }
        public List<Site>        Sites      { get; set; }
    }
}
