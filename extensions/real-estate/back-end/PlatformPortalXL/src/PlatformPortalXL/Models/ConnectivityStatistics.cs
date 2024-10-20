using System;
using System.Collections.Generic;

namespace PlatformPortalXL.Models
{
    public class ConnectivityStatistics
    {
        public Guid SiteId { get; set; }
        public List<Gateway> gateways { get; set; }
        public List<Connector> connectors { get; set; }
    }
}
