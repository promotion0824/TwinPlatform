using PlatformPortalXL.Features.Pilot;
using System.Collections.Generic;
using System.Diagnostics;

namespace PlatformPortalXL.Dto;

[DebuggerDisplay("{Twin?.Name}, Children = {Children?.Count}")]
public class NestedTwinDto
{
    public TwinDto Twin { get; set; }
    public IList<NestedTwinDto> Children { get; set; }
}
