using System.Text.Json.Serialization;
using Willow.HealthChecks;
using Willow.Hosting.Web;
using Willow.Msm.Connector;
using Willow.Msm.Connector.Services;

return WebApplicationStart.Run(args, "MsmConnector", Configure, ConfigureApp, ConfigureHealthChecks);

void Configure(WebApplicationBuilder builder)
{
    builder.Services.AddHttpClient();
    builder.Services.AddSingleton<IWillowClient, WillowClient>();

    builder.Services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });
}

ValueTask ConfigureApp(WebApplication app)
{
    // Configure endpoint
    app.MapPost("/carbon-activity", CarbonActivity.ProcessCarbonActivityRequest);
    return ValueTask.CompletedTask;
}

void ConfigureHealthChecks(IHealthChecksBuilder healthChecksBuilder)
{
    // Add health check for Public API
    healthChecksBuilder.AddPublicApi();
}
