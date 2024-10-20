using System;
using System.Collections.Generic;

namespace PlatformPortalXL.Models
{
    public class Gateway
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid CustomerId { get; set; }
        public Guid SiteId { get; set; }
        public string Host { get; set; }
        public bool IsEnabled { get; set; }
        public bool? IsOnline { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public List<Connector> Connectors { get; set; }
    }
}
