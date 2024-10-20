using System;

namespace WorkflowCore.Models
{
    public class Site
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public string Name { get; set; }
        public SiteFeatures Features { get; set; }
        public string TimezoneId { get; set; }
    }
}
