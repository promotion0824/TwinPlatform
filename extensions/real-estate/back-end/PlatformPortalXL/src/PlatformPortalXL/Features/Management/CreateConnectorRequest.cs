using System;

namespace PlatformPortalXL.Features.Management
{
    public class CreateConnectorRequest
    {
        public string Configuration { get; set; }
        public string Name { get; set; }
        public Guid ConnectorTypeId { get; set; }
        public string ConnectionType { get; set; }
    }
}
