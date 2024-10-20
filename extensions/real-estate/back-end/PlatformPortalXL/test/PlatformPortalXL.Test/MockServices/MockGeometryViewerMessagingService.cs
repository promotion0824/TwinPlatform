using PlatformPortalXL.Services.GeometryViewer;
using System;
using System.Threading.Tasks;

namespace PlatformPortalXL.Test.MockServices
{
    public class MockGeometryViewerMessagingService : IGeometryViewerMessagingService
    {
        public MockGeometryViewerMessagingService()
        {
        }

        public Task Send(Guid siteId, string twinId, string urn)
        {
            return Task.CompletedTask;
        }

        public Task Send(IGeometryViewerMessage message)
        {
            return Task.CompletedTask;
        }
    }
}
