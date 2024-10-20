using System;
using System.Threading.Tasks;
using Willow.Common;

namespace PlatformPortalXL.Services.GeometryViewer
{
    public class GeometryViewerMessagingService : IGeometryViewerMessagingService
    {
        private readonly IMessageQueue _msgQueue;

        public GeometryViewerMessagingService(
            IMessageQueue msgQueue)
        {
            _msgQueue = msgQueue;
        }

        public async Task Send(Guid siteId, string twinId, string urn)
        {
            if (_msgQueue != null)
            {
                await _msgQueue.Send(new
                {
                    SiteId = siteId,
                    TwinId = twinId,
                    Urn = urn
                });
            }
        }

        public Task Send(IGeometryViewerMessage message)
        {
            return Send(message.SiteId, message.TwinId, message.Urn);
        }
    }
}
