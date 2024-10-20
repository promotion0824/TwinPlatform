namespace Willow.IoTService.Deployment.Dashboard.Infrastructure;

public static class WebApplicationBuilderExtensions
{
    public static void UseSwaggerAndUIWithAad(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(
                         c =>
                         {
                             c.OAuthClientId(app.Configuration["AzureAdB2C:ClientId"]);
                             c.OAuthUsePkce();
                             c.OAuthScopeSeparator(" ");
                         });
    }

    public static void MapAllHealthChecks(this WebApplication app)
    {
        app.MapHealthChecks("/healthcheck");
        app.MapHealthChecks("/healthz");
        app.MapHealthChecks("/health");
    }
}
