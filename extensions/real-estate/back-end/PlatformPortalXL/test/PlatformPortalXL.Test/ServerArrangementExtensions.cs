using Microsoft.Extensions.DependencyInjection;
using PlatformPortalXL.Services;
using System;

using Willow.Common;
using Willow.Tests.Infrastructure.MockServices;

namespace Willow.Tests.Infrastructure
{
    public static class ServerArrangementExtensions
    {
        public static ServerArrangement SetCurrentDateTime(this ServerArrangement arrangement, DateTime currentDateTime)
        {
            var service = (MockDateTimeService)arrangement.MainServices.GetRequiredService<IDateTimeService>();
            service.UtcNow = currentDateTime;
            return arrangement;
        }

        public static IImageUrlHelper GetImageUrlHelper(this ServerArrangement arrangement)
        {
            return arrangement.MainServices.GetRequiredService<IImageUrlHelper>();
        }

        public static DependencyServiceHttpHandler GetDirectoryApi(this ServerArrangement arrangement)
        {
            return arrangement.GetHttpHandler(ApiServiceNames.DirectoryCore);
        }

        public static DependencyServiceHttpHandler GetSiteApi(this ServerArrangement arrangement)
        {
            return arrangement.GetHttpHandler(ApiServiceNames.SiteCore);
        }

        public static DependencyServiceHttpHandler GetConnectorApi(this ServerArrangement arrangement)
        {
            return arrangement.GetHttpHandler(ApiServiceNames.ConnectorCore);
        }

        public static DependencyServiceHttpHandler GetLiveDataApi(this ServerArrangement arrangement)
        {
            return arrangement.GetHttpHandler(ApiServiceNames.LiveDataCore);
        }

        public static DependencyServiceHttpHandler GetWorkflowApi(this ServerArrangement arrangement)
        {
            return arrangement.GetHttpHandler(ApiServiceNames.WorkflowCore);
        }

        public static DependencyServiceHttpHandler GetInsightApi(this ServerArrangement arrangement)
        {
            return arrangement.GetHttpHandler(ApiServiceNames.InsightCore);
        }

        public static DependencyServiceHttpHandler GetAssetApi(this ServerArrangement arrangement)
        {
            return arrangement.GetHttpHandler(ApiServiceNames.AssetCore);
        }

        public static DependencyServiceHttpHandler GetDigitalTwinApi(this ServerArrangement arrangement)
        {
            return arrangement.GetHttpHandler(ApiServiceNames.DigitalTwinCore);
        }

		public static DependencyServiceHttpHandler GetArcGisApi(this ServerArrangement arrangement)
		{
			return arrangement.GetHttpHandler(ApiServiceNames.ArcGis);
		}

		public static DependencyServiceHttpHandler GetMarketPlaceApi(this ServerArrangement arrangement)
        {
            return arrangement.GetHttpHandler(ApiServiceNames.MarketPlaceCore);
        }

        public static DependencyServiceHttpHandler GetConnectorExporterApi(this ServerArrangement arrangement)
        {
            return arrangement.GetHttpHandler(ApiServiceNames.ConnectorExporter);
        }

        public static DependencyServiceHttpHandler GetConnectorImporterApi(this ServerArrangement arrangement)
        {
            return arrangement.GetHttpHandler(ApiServiceNames.ConnectorImporter);
        }
        public static DependencyServiceHttpHandler GetNotificationApi(this ServerArrangement arrangement)
        {
            return arrangement.GetHttpHandler(ApiServiceNames.NotificationCore);
        }
    }
}
