using System;
using System.Collections.Generic;

namespace DirectoryCore.Domain
{
    public class Portfolio
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid CustomerId { get; set; }
        public PortfolioFeatures Features { get; set; }
        public List<Site> Sites { get; set; }
    }
}
