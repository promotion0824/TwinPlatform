using System;
using System.Threading.Tasks;

namespace PlatformPortalXL.Services.GeometryViewer
{
    public interface IGeometryViewerMessagingService
    {
        Task Send(Guid siteId, string twinId, string urn);
        Task Send(IGeometryViewerMessage message);
    }
}
