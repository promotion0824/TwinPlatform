using System;

namespace MobileXL.Models
{
    public class Site
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Address { get; set; }
        public string TimeZoneId { get; set; }
        public Customer Customer { get; set; }
        public SiteFeatures Features { get; set; } = new SiteFeatures();
    }
}
