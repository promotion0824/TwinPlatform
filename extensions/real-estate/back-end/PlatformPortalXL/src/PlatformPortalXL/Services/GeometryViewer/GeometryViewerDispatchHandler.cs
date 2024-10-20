using Microsoft.Extensions.Logging;
using PlatformPortalXL.Helpers;
using PlatformPortalXL.ServicesApi.GeometryViewerApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Willow.MessageDispatch;
using Willow.TimedDispatch;

namespace PlatformPortalXL.Services.GeometryViewer
{
    public class GeometryViewerDispatchHandler : IMessageDispatchHandler, ITimedDispatchHandler
    {
        private readonly IGeometryViewerService _geometryViewerService;
        private readonly IGeometryViewerApiService _geometryViewerApi;
        private readonly IGeometryViewerMessagingService _geometryViwerMessagingService;
        private readonly ILogger<GeometryViewerDispatchHandler> _logger;

        public GeometryViewerDispatchHandler(
            IGeometryViewerService geometryViewerService,
            IGeometryViewerApiService geometryViewerApi,
            IGeometryViewerMessagingService geometryViewerMessagingService,
            ILogger<GeometryViewerDispatchHandler> logger)
        {
            _geometryViewerService = geometryViewerService ?? throw new ArgumentNullException(nameof(geometryViewerService));
            _geometryViewerApi = geometryViewerApi ?? throw new ArgumentNullException(nameof(geometryViewerApi));
            _geometryViwerMessagingService = geometryViewerMessagingService ?? throw new ArgumentNullException(nameof(geometryViewerMessagingService));

            _logger = logger;
        }

		class GeometryViewerMessage : IGeometryViewerMessage
		{
			public Guid SiteId { get; set; }
			public string TwinId { get; set; }
			public string Urn { get; set; }
		}

		public async void OnMessageDispatch(object sender, MessageDispatchEventArgs e)
        {
            try
            {
                var message = JsonSerializerHelper.Deserialize<GeometryViewerMessage>(e.Message);

				await Process(new List<IGeometryViewerMessage> { message });
            } 
            catch (Exception ex)
            {
                _logger.LogError("Failed to process geometry viewer dispatched message {Message}: {ExceptionMessage}", e.Message, ex.Message);
            }
        }

        public Task OnTimedDispatch(object state)
        {
            // TODO: Fetch the urns of existing (uninspected) models. How do we do it without a user context? How do we remember which ones have been inspected? 
            // Call Process 
            return Task.CompletedTask;
        }

        private async Task Process(List<IGeometryViewerMessage> messages) 
        {
            foreach (var message in messages)
            {
                if (!await _geometryViewerApi.ExistsGeometryViewerModel(message.Urn))
                {
                    var geometryViewerIds = await _geometryViewerService.GetGeometryViewerIds(message.Urn);

                    if (geometryViewerIds == null)
                    {
                        await _geometryViwerMessagingService.Send(message);
                    }
                    else
                    {
                        var references = geometryViewerIds.Select(x => new GeometryViewerReference { GeometryViewerId = x }).ToList();

                        await _geometryViewerApi.AddGeometryViewerModel(new GeometryViewerModel
                        {
                            Is3D = true,
                            References = references,
                            TwinId = message.TwinId,
                            Urn = message.Urn
                        });
                    }
                }
            }
        }
    }
}
