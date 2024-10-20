namespace Willow.Tests.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Mime;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public class MainServiceStartup<TOriginalStartup>
        where TOriginalStartup : class
    {
        private readonly TOriginalStartup startup;

        public MainServiceStartup(IServiceProvider serviceProvider)
        {
            startup = serviceProvider.CreateInstance<TOriginalStartup>();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();
            var serverFixtureConfiguration = serviceProvider.GetRequiredService<ServerFixtureConfiguration>();

            serviceProvider.InvokeMethod(
                startup,
                "ConfigureServices",
                new Dictionary<Type, object>() { [typeof(IServiceCollection)] = services });

            if (serverFixtureConfiguration.EnableTestAuthentication)
            {
                services.AddTestAuthentication();
            }

            services.AddMvc().AddApplicationPart(typeof(TOriginalStartup).Assembly);

            serverFixtureConfiguration.MainServicePostConfigureServices?.Invoke(services);
        }

        public void Configure(IApplicationBuilder app, IServiceProvider serviceProvider)
        {
            // TestServer does not return 500 when an internal exception pops up, it passes the exception to the caller.
            // Add this middleware to simulate a real server behavior: returns status code 500.
            //app.UseMiddleware<ExceptionMiddleware>();
            serviceProvider.InvokeMethod(
                startup,
                "Configure",
                new Dictionary<Type, object>() { [typeof(IApplicationBuilder)] = app });
        }

        public class ExceptionMiddleware
        {
            private readonly RequestDelegate next;
            private readonly ILogger<ExceptionMiddleware> logger;

            public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
            {
                this.next = next;
                this.logger = logger;
            }

            public async Task Invoke(HttpContext httpContext)
            {
                try
                {
                    await next(httpContext);
                }
                catch (Exception ex)
                {
                    httpContext.Response.ContentType = MediaTypeNames.Text.Plain;
                    httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    logger.LogError(ex, "Internal server error");
                    await httpContext.Response.WriteAsync("Internal server error: " + ex.ToString());
                }
            }
        }
    }
}
