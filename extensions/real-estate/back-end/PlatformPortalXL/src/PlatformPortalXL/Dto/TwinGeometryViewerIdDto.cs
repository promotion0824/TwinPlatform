using System;

namespace PlatformPortalXL.Dto;

public class TwinGeometryViewerIdDto
{
    public string TwinId { get; set; }
    public Guid? UniqueId { get; set; }
    public Guid? GeometryViewerId { get; set; }
}
