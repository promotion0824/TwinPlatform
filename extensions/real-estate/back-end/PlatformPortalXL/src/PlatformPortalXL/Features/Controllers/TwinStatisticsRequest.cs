using System;
using System.Collections.Generic;

namespace PlatformPortalXL.Features.Controllers;

public class TwinStatisticsRequest
{
    public Guid SiteId { get; set; }
    public Guid? FloorId { get; set; }
    public List<string> ModuleTypeNamePaths { get; set; }
}
