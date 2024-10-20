using System;
using System.Collections.Generic;

namespace PlatformPortalXL.Dto
{
    public class ConnectorTypeDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<ConnectorTypeColumnDto> Columns { get; set; }
    }

    public class ConnectorTypeColumnDto
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public bool IsRequired { get; set; }
    }
}
