namespace Willow.PublicApi.Extensions;

using global::Authorization.TwinPlatform.Common.Abstracts;
using global::Authorization.TwinPlatform.Common.Model;
using Willow.PublicApi.Authorization;

internal static class ImportUserManagementConfigExtension
{
    public static WebApplication ImportUserManagementConfig(this WebApplication app)
    {
        var userManagementImportService = app.Services.GetRequiredService<IImportService>();
        userManagementImportService.ImportDataFromConfiguration(new ImportModel()
        {
            ApplicationOption = new()
            {
                Description = "Public API",
                SupportClientAuthentication = true,
            },
            Permissions = [.. Permissions.ImportPermissions.Values],
        });
        return app;
    }
}
