using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PlatformPortalXL.Helpers;
using PlatformPortalXL.Models;
using PlatformPortalXL.Services.MarketPlaceApi;
using PlatformPortalXL.ServicesApi.ConnectorApi;
using PlatformPortalXL.ServicesApi.SiteApi;

namespace PlatformPortalXL.Services
{
    public interface IAppManagementService
    {
        Task InstallApp(Guid siteId, Guid appId, Guid installedByUserId);
        Task UninstallApp(Guid siteId, Guid appId);
    }

    public class AppManagementService : IAppManagementService
    {
        private readonly IMarketPlaceApiService _marketPlaceApi;
        private readonly IConnectorApiService _connectorApi;
        private readonly ISiteApiService _siteApi;
        private readonly ILogger<AppManagementService> _logger;

        public AppManagementService(
            IMarketPlaceApiService marketPlaceApi,
            IConnectorApiService connectorApi, 
            ISiteApiService siteApi, 
            ILogger<AppManagementService> logger)
        {
            _marketPlaceApi = marketPlaceApi;
            _connectorApi = connectorApi;
            _siteApi = siteApi;
            _logger = logger;
        }

        public async Task InstallApp(Guid siteId, Guid appId, Guid installedByUserId)
        {
            var app = await _marketPlaceApi.GetApp(appId);

            await _marketPlaceApi.InstallApp(siteId, appId, installedByUserId);

            var manifest = JsonSerializerHelper.Deserialize<AppManifest>(app.ManifestJson);

            if (manifest.Capabilities.Contains(Capabilities.ProvidePoints)) //if it's a connector app
            {
                Connector existingConnector;
                try
                {
                    existingConnector = await _connectorApi.GetConnectorByTypeAsync(siteId, appId);
                }
                catch (InvalidDataException ex)
                {
                    _logger.LogWarning(
                        ex,
                        "Couldn't correctly install connector. app {AppId} siteId {SiteId}. there are more than one existing connectors of the same type",
                        app.Id,
                        siteId);
                    return;
                }

                if (existingConnector == null)
                {
                    var site = await _siteApi.GetSite(siteId);

                    var connectorType =
                        await _connectorApi
                            .GetConnectorTypeAsync(app.Id); //application's id correlates with connector type's id

                    var connectorMetadataTemplate =
                        await _connectorApi.GetSchemaTemplateAsync(connectorType.ConnectorConfigurationSchemaId);

                    var newConnector = new Connector
                    {
                        ClientId = site.CustomerId,
                        ConnectorTypeId = app.Id,
                        ErrorThreshold = 10,
                        IsEnabled = true,
                        IsLoggingEnabled = true,
                        Name = $"{app.Name} Connector for {site.Name}",
                        SiteId = siteId,
                        Configuration = connectorMetadataTemplate
                    };

                    await _connectorApi.CreateConnectorAsync(newConnector);
                }
                else
                {
                    if (!existingConnector.IsEnabled)
                    {
                        await _connectorApi.SetConnectorEnabled(existingConnector.Id, true);
                    }
                }
            }


        }

        public async Task UninstallApp(Guid siteId, Guid appId)
        {
            var app = await _marketPlaceApi.GetApp(appId);
            var manifest = JsonSerializerHelper.Deserialize<AppManifest>(app.ManifestJson);

           

            await _marketPlaceApi.UninstallApp(siteId, appId);

            if (manifest.Capabilities.Contains(Capabilities.ProvidePoints)) //if it's a connector app
            {
                try
                {
                    var connector = await _connectorApi.GetConnectorByTypeAsync(siteId, app.Id);
                    if (connector == null)
                    {
                        return;
                    }

                    await _connectorApi.SetConnectorEnabled(connector.Id, false);
                }
                catch (InvalidDataException ex)
                {
                    _logger.LogWarning(
                        ex,
                        "Couldn't correctly disable connector. app {AppId} siteId {SiteId}. there are more than one existing connectors of the same type",
                        app.Id,
                        siteId);
                }
            }
        }
    }
}
