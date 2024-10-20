using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Contrib.HttpClient;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Threading.Tasks;
using Willow.Common;
using Willow.Infrastructure.MultiRegion;
using Willow.Notifications.Interfaces;
using Willow.Tests.Infrastructure.MockServices;
using WorkflowCore.Services;
using WorkflowCore.Test.Infrastructure.MockServices;

namespace Willow.Tests.Infrastructure
{
    public static class EmailContainer
    {
        public static Mock<INotificationService> NotificationService  = new();
    }

    public class MainServiceStartup<TOriginalStartup> where TOriginalStartup : class
    {
        private readonly TOriginalStartup _startup;

        public MainServiceStartup(IServiceProvider serviceProvider)
        {
            _startup = serviceProvider.CreateInstance<TOriginalStartup>();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.InvokeMethod(
                _startup,
                "ConfigureServices",
                new Dictionary<Type, object>() { [typeof(IServiceCollection)] = services });

            var assembly = typeof(TOriginalStartup).Assembly;
            services.AddApplicationPart(assembly);

            var logger = serviceProvider.GetRequiredService<ILogger<MainServiceStartup<TOriginalStartup>>>();
            var serverFixtureConfiguration = serviceProvider.GetRequiredService<ServerFixtureConfiguration>();

            if (serverFixtureConfiguration.EnableTestAuthentication)
            {
                services.AddTestAuthentication();
            }

            var httpHandler = new Mock<HttpMessageHandler>();
            httpHandler.SetupRequest(message =>
            {
                logger.LogInformation("MockHttpRequest: {method} {url}", message.Method, message.RequestUri.ToString());
                return false;
            });
            services.AddSingleton(httpHandler);
            var httpClientFactory = httpHandler.CreateClientFactory();
            services.AddSingleton(typeof(IHttpClientFactory), httpClientFactory);
            var mockHttpClientFactory = Mock.Get(httpClientFactory);
            var needMultiRegion = false;
            foreach (var dependencyService in serverFixtureConfiguration.DependencyServices)
            {
                if (dependencyService.IsMultiRegion)
                {
                    foreach (var regionId in serverFixtureConfiguration.RegionIds)
                    {
                        var serviceName = MultiRegionHelper.ServiceName(dependencyService.Name, regionId);
                        ConfigureDependencyService(serviceName, dependencyService, httpHandler, mockHttpClientFactory, logger);
                        needMultiRegion = true;
                    }
                }
                else
                {
                    ConfigureDependencyService(dependencyService.Name, dependencyService, httpHandler, mockHttpClientFactory, logger);
                }
            }

            EmailContainer.NotificationService.Setup( s=> s.SendNotificationAsync(It.IsAny<Willow.Notifications.Models.Notification>())).Returns(Task.CompletedTask);

            services.ReplaceScoped<INotificationService>( p=> EmailContainer.NotificationService.Object);
            services.ReplaceScoped<IPushNotificationServer, MockPushNotificationServer>();
            services.ReplaceSingleton<IDateTimeService, MockDateTimeService>();
			services.ReplaceScoped<ISessionService, MockSessionService>();
            if (needMultiRegion)
            {
                services.AddSingleton<IMultiRegionSettings, MockMultiRegionSettings>();
            }

            var mockWorkflowSequenceNumberService = new Mock<IWorkflowSequenceNumberService>();
            mockWorkflowSequenceNumberService.Setup(s => s.GenerateSequenceNumber(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((string prefix, string key) => prefix + "-" + key + "-1");
            services.ReplaceScoped(_ => mockWorkflowSequenceNumberService.Object);

            serverFixtureConfiguration.MainServicePostConfigureServices?.Invoke(services);
        }

        private void ConfigureDependencyService(
            string serviceName,
            DependencyServiceConfiguration dependencyService,
            Mock<HttpMessageHandler> mockHttpHandler,
            Mock<IHttpClientFactory> mockHttpClientFactory,
            ILogger logger)
        {
            var baseUrl = $"https://{serviceName}.com";
            mockHttpClientFactory
                .Setup(x => x.CreateClient(serviceName))
                .Returns(() =>
                {
                    var client = mockHttpHandler.CreateClient();
                    client.BaseAddress = new Uri(baseUrl);
                    return client;
                });
            if (dependencyService.Setup != null)
            {
                var handler = new DependencyServiceHttpHandler(mockHttpHandler, baseUrl);
                dependencyService.Setup(handler);
            }
        }

        public void Configure(IApplicationBuilder app, IServiceProvider serviceProvider)
        {
            // TestServer does not return 500 when an internal exception pops up, it passes the exception to the caller.
            // Add this middleware to simulate a real server behavior: returns status code 500.
            app.UseMiddleware<ExceptionMiddleware>();

            serviceProvider.InvokeMethod(
                _startup,
                "Configure",
                new Dictionary<Type, object>() { [typeof(IApplicationBuilder)] = app });
        }

        public class ExceptionMiddleware
        {
            private readonly RequestDelegate _next;
            private readonly ILogger<ExceptionMiddleware> _logger;

            public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
            {
                _next = next;
                _logger = logger;
            }

            public async Task Invoke(HttpContext httpContext)
            {
                try
                {
                    await _next(httpContext);
                }
                catch (Exception ex)
                {
                    httpContext.Response.ContentType = MediaTypeNames.Text.Plain;
                    httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    _logger.LogError(ex, "Internal server error");
                    await httpContext.Response.WriteAsync("Internal server error: " + ex.ToString());
                }
            }
        }
    }

}
