//-----------------------------------------------------------------------
// <copyright file="Startup.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace MSMConnectorApi
{
    using Microsoft.AspNetCore.Mvc.ApiExplorer;
    using Microsoft.Extensions.Options;
    using Microsoft.OpenApi.Models;
    using Swashbuckle.AspNetCore.SwaggerGen;

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseAuthorization();

            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    var apiVersionDescriptionProvider = app.ApplicationServices.GetRequiredService<IApiVersionDescriptionProvider>();

                    foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions.Reverse())
                    {
                        options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json",
                          description.GroupName);
                    }
                });
            }

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
