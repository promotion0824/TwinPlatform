using System;
using System.IO;
using System.Net.Http;
using System.Text;

using Xunit;
using Moq;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Willow.Common;

using MobileXL.Services.Apis.DigitalTwinApi;

namespace MobileXL.UnitTests
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

            services.AddSingleton<IConfiguration>( p=> configuration);
            services.AddDigitalTwinApi(configuration);
            services.AddSingleton<IBlobStore, MemoryStore>();
            services.AddSingleton<IHttpClientFactory>(httpFactory.Object);

            var serviceProvider = services.BuildServiceProvider();

            var dtService = serviceProvider.GetService<IDigitalTwinApiService>();

            Assert.NotNull(dtService);
        }
    }
}
