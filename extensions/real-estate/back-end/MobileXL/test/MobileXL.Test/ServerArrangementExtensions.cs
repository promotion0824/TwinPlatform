using Microsoft.Extensions.DependencyInjection;
using MobileXL.Services;
using MobileXL.Services.Apis;
using System;
using Willow.Tests.Infrastructure.MockServices;

using Willow.Common;

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

        public static DependencyServiceHttpHandler GetMarketPlaceApi(this ServerArrangement arrangement)
        {
            return arrangement.GetHttpHandler(ApiServiceNames.MarketPlaceCore);
        }

        public static DependencyServiceHttpHandler GetInsightApi(this ServerArrangement arrangement)
        {
            return arrangement.GetHttpHandler(ApiServiceNames.InsightCore);
        }

        public static DependencyServiceHttpHandler GetWorkflowApi(this ServerArrangement arrangement)
        {
            return arrangement.GetHttpHandler(ApiServiceNames.WorkflowCore);
        }

        public static DependencyServiceHttpHandler GetDigitalTwinApi(this ServerArrangement arrangement)
        {
            return arrangement.GetHttpHandler(ApiServiceNames.DigitalTwinCore);
        }
    }
}
