using System;

namespace PlatformPortalXL.Services.GeometryViewer
{
    public interface IGeometryViewerMessage
    {
        Guid SiteId { get; }
        string TwinId { get; }
        string Urn { get; }
    }
}
