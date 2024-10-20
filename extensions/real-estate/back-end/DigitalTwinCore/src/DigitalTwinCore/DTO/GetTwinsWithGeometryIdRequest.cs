using System.Collections.Generic;
using System;

namespace DigitalTwinCore.DTO;

public class GetTwinsWithGeometryIdRequest
{
    public Guid SiteId { get; set; }
    public Guid? FloorId { get; set; }
    public List<string> ModuleTypeNamePaths { get; set; }
}
