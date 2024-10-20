using System;
using System.IO;
using System.Net.Http;
using System.Text;

using Xunit;
using Moq;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Willow.Common;
using Willow.Platform.Statistics;

using PlatformPortalXL.Features;
using PlatformPortalXL.ServicesApi.DigitalTwinApi;
using Willow.Api.AzureStorage;
using Willow.PlatformPortalXL;

namespace Willow.PlatformPortal.XL.UnitTests
{
    public class DependencyInjectionTests
    {
        [Fact]
        public void DependencyInjection_success()
        {
            var services = new ServiceCollection();

            // "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;";        
            var appSettings = @"{
                                 ""Azure:BlobStorage:AccountName"":   ""devstoreaccount1"",
                                 ""Azure:BlobStorage:AccountKey"":    ""Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw=="",
                                 ""Azure:BlobStorage:ContainerName"": ""test""
                                }";

            var builder = new ConfigurationBuilder();

            builder.AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(appSettings)));

            var configuration = builder.Build();
            var httpFactory = new Mock<IHttpClientFactory>();

            services.AddMemoryCache();
            services.AddSingleton<IConfiguration>( p=> configuration);
            services.AddSingleton<IHttpClientFactory>(httpFactory.Object);
            services.AddKPIService(configuration);
            services.AddDigitalApiService(configuration);
            services.AddStatisticsService();
            services.AddLogging();

            var serviceProvider = services.BuildServiceProvider();

           // var kpiSvc =serviceProvider.GetService<IKPIServiceFactory>();
           // var dtService = serviceProvider.GetService<IDigitalTwinApiService>();
            var stats = serviceProvider.GetService<IStatisticsService>();

           // Assert.NotNull(kpiSvc);
          //  Assert.NotNull(dtService);
            Assert.NotNull(stats);
        }
    }
}
