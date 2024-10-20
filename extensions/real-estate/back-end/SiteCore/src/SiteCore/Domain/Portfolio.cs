using System;
using System.Collections.Generic;

namespace SiteCore.Domain
{
    public class Portfolio
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid CustomerId { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public ICollection<Site> Sites { get; set; }
    }
}
