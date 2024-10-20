using System;

namespace PlatformPortalXL.Models
{
    public class ConnectorTypeColumn
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool IsRequired { get; set; }
        public string DataType { get; set; }
    }
}
