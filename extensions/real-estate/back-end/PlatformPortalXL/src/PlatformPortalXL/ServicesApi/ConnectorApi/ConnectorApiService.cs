using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using PlatformPortalXL.Extensions;
using PlatformPortalXL.Features.Management;
using PlatformPortalXL.Models;
using PlatformPortalXL.Services;

namespace PlatformPortalXL.ServicesApi.ConnectorApi
{
    public interface IConnectorApiService
    {
        Task<List<Equipment>> GetSiteEquipments(Guid siteId);
        Task<List<Category>> GetCategoriesBySiteId(Guid siteId);
        Task<Device> GetDeviceAsync(Guid siteId, Guid deviceId);
        Task<List<Equipment>> GetEquipmentsByCategory(Guid siteId, Guid categoryId);
        Task<List<Equipment>> SearchEquipmentsAsync(Guid siteId, Guid floorId, string keyword, bool includeEquipmentsNotLinkedToFloor);

        Task<Equipment> GetEquipment(Guid equipmentId, bool includePoints, bool includePointTags);
        Task<Point> GetPoint(Guid pointEntityId);
        Task<Point> GetPointById(Guid pointId);
        Task<IList<Point>> GetPoints(Guid[] pointEntityIds);
        Task<Connector> GetConnectorById(Guid siteId, Guid connectorId);
        Task<Connector> CreateConnectorAsync(Connector newConnector);
        Task<Connector> UpdateConnectorAsync(Connector connector);
        Task<ConnectorType> GetConnectorTypeAsync(Guid connectorTypeId);
        Task<List<ConnectorType>> GetConnectorTypesAsync();
        Task<List<Connector>> GetConnectorsAsync(Guid siteId, bool includePointsCount);
        Task<List<ConnectorLogRecord>> GetLatestConnectorLogsAsync(Guid connectorId, int count, bool includeErrors, string source);
        Task<List<ConnectorScan>> GetConnectorScansAsync(Guid connectorId);
        Task<LogErrorsCore> GetConnectorLogErrorsAsync(Guid connectorId, long logId);
        Task<List<ConnectorLogRecord>> GetLogsForConnectorAsync(Guid connectorId, DateTime start, DateTime end);
        Task<List<Gateway>> GetGatewaysForSiteAsync(Guid siteId);
        Task<List<ConnectivityStatistics>> GetGatewaysForMultiSitesAsync(List<Guid> siteIds);
        Task<string> GetSchemaTemplateAsync(Guid schemaId);
        Task<List<ConnectorTypeColumn>> GetSchemaColumnsAsync(Guid schemaId);
        Task<Connector> GetConnectorByTypeAsync(Guid siteId, Guid connectorTypeId);
        Task SetConnectorEnabled(Guid connectorId, bool enabled);
        Task<List<Equipment>> GetSiteEquipmentsWithCategory(Guid siteId);
        Task<ConnectorScan> CreateConnectorScanAsync(Guid connectorId, ConnectorScan connectorScan);
        Task UpdateConnectorScanAsync(Guid connectorId, Guid scanId, UpdateConnectorScanRequest request);
        Task<ConnectorScan> GetConnectorScanByIdAsync(Guid connectorId, Guid scanId);
        Task StopConnectorScanAsync(Guid connectorId, Guid scanId);
        Task<Stream> DownloadScanResultAsStream(Guid connectorId, Guid scanId);
        Task<SetPointCommand> GetSetPointCommandAsync(Guid siteId, Guid setPointCommandId);
        Task<List<SetPointCommand>> GetSetPointCommandsAsync(Guid siteId, Guid? insightId, Guid? equipmentId);
        Task<SetPointCommand> CreateSetPointCommandAsync(SetPointCommand setPointCommand);
        Task<SetPointCommand> UpdateSetPointCommandAsync(SetPointCommand setPointCommand);
        Task DeleteSetPointCommandAsync(Guid siteId, Guid setPointCommandId);
        Task<List<SetPointCommandConfiguration>> GetSetPointCommandConfigurationsAsync();
        Task SetConnectorArchived(Guid connectorId, bool archive);
    }

    public class ConnectorApiService : IConnectorApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ConnectorApiService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<List<Equipment>> GetSiteEquipments(Guid siteId)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.ConnectorCore))
            {
                var response = await client.GetAsync($"sites/{siteId}/allEquipments");
                await response.EnsureSuccessStatusCode(ApiServiceNames.ConnectorCore);
                return EquipmentCore.MapToModels(await response.Content.ReadAsAsync<List<EquipmentCore>>());
            }
        }

        public async Task<List<Category>> GetCategoriesBySiteId(Guid siteId)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.ConnectorCore))
            {
                var response = await client.GetAsync($"sites/{siteId}/equipments/categories");
                await response.EnsureSuccessStatusCode(ApiServiceNames.ConnectorCore);
                return await response.Content.ReadAsAsync<List<Category>>();
            }
        }

        public async Task<List<Equipment>> GetEquipmentsByCategory(Guid siteId, Guid categoryId)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.ConnectorCore))
            {
                var url = $"sites/{siteId}/categories/{categoryId}/equipments";
                var response = await client.GetAsync(url);
                await response.EnsureSuccessStatusCode(ApiServiceNames.ConnectorCore);
                return EquipmentCore.MapToModels(await response.Content.ReadAsAsync<List<EquipmentCore>>());
            }
        }

        public async Task<List<Equipment>> SearchEquipmentsAsync(Guid siteId, Guid floorId, string keyword, bool includeEquipmentsNotLinkedToFloor)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.ConnectorCore))
            {
                var url = $"sites/{siteId}/floors/{floorId}/equipments";
                url = QueryHelpers.AddQueryString(url, "keyword", keyword ?? string.Empty);
                url = QueryHelpers.AddQueryString(url, "includeEquipmentsNotLinkedToFloor", includeEquipmentsNotLinkedToFloor.ToString(CultureInfo.InvariantCulture));
                var response = await client.GetAsync(url);
                await response.EnsureSuccessStatusCode(ApiServiceNames.ConnectorCore);
                return EquipmentCore.MapToModels(await response.Content.ReadAsAsync<List<EquipmentCore>>());
            }
        }

        public async Task<Equipment> GetEquipment(Guid equipmentId, bool includePoints, bool includePointTags)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.ConnectorCore))
            {
                var response = await client.GetAsync($"equipments/{equipmentId}?includePoints={includePoints}&includePointTags={includePointTags}");
                await response.EnsureSuccessStatusCode(ApiServiceNames.ConnectorCore);
                return EquipmentCore.MapToModel(await response.Content.ReadAsAsync<EquipmentCore>());
            }
        }

        public async Task<Point> GetPoint(Guid pointEntityId)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.ConnectorCore))
            {
                var response = await client.GetAsync($"points/{pointEntityId}");
                await response.EnsureSuccessStatusCode(ApiServiceNames.ConnectorCore);
                var pointCore = await response.Content.ReadAsAsync<PointCore>();
                return PointCore.MapToModel(pointCore);
            }
        }

        public async Task<Point> GetPointById(Guid pointId)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.ConnectorCore))
            {
                var response = await client.GetAsync($"temp/points/{pointId}");
                await response.EnsureSuccessStatusCode(ApiServiceNames.ConnectorCore);
                var pointCore = await response.Content.ReadAsAsync<PointCore>();
                return PointCore.MapToModel(pointCore);
            }
        }

        public async Task<IList<Point>> GetPoints(Guid[] pointEntityIds)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.ConnectorCore))
            {
                var uriBuilder = new StringBuilder($"points?")
                    .Append(string.Join("&", pointEntityIds.Select(x => $"pointEntityId={x}")));
                var response = await client.GetAsync(uriBuilder.ToString());
                await response.EnsureSuccessStatusCode(ApiServiceNames.ConnectorCore);
                var pointCore = await response.Content.ReadAsAsync<PointCore[]>();
                return PointCore.MapToModels(pointCore);
            }
        }

        public async Task<Connector> GetConnectorById(Guid siteId, Guid connectorId)
        {
            using var client = _httpClientFactory.CreateClient(ApiServiceNames.ConnectorCore);
            var response = await client.GetAsync($"sites/{siteId}/connectors/{connectorId}");
            await response.EnsureSuccessStatusCode(ApiServiceNames.ConnectorCore);
            return await response.Content.ReadAsAsync<Connector>();
        }

        public async Task<Connector> CreateConnectorAsync(Connector newConnector)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.ConnectorCore))
            {
                var response = await client.PostFormAsync($"connectors", newConnector);
                await response.EnsureSuccessStatusCode(ApiServiceNames.ConnectorCore);
                var connector = await response.Content.ReadAsAsync<Connector>();
                return connector;
            }
        }

        public async Task<Connector> UpdateConnectorAsync(Connector connector)
        {
            using var client = _httpClientFactory.CreateClient(ApiServiceNames.ConnectorCore);
            var response = await client.PutFormAsync($"connectors", connector);
            await response.EnsureSuccessStatusCode(ApiServiceNames.ConnectorCore);
            var updatedConnector = await response.Content.ReadAsAsync<Connector>();
            return updatedConnector;
        }

        public async Task<List<ConnectorTypeColumn>> GetSchemaColumnsAsync(Guid schemaId)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.ConnectorCore))
            {
                var response = await client.GetAsync($"schemas/{schemaId}/SchemaColumns");
                await response.EnsureSuccessStatusCode(ApiServiceNames.ConnectorCore);
                return await response.Content.ReadAsAsync<List<ConnectorTypeColumn>>();
            }
        }

        public async Task<Connector> GetConnectorByTypeAsync(Guid siteId, Guid connectorTypeId)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.ConnectorCore))
            {
                var response = await client.GetAsync($"sites/{siteId}/connectors/bytype/{connectorTypeId}");
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }

                if (response.StatusCode == HttpStatusCode.Conflict)
                {
                    throw new InvalidDataException();
                }
                await response.EnsureSuccessStatusCode(ApiServiceNames.ConnectorCore);
                var connector = await response.Content.ReadAsAsync<Connector>();
                return connector;
            }
        }

        public async Task<List<Connector>> GetConnectorsAsync(Guid siteId, bool includePointsCount)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.ConnectorCore))
            {
                var response = await client.GetAsync($"sites/{siteId}/connectors?includePointsCount={includePointsCount}");
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return Enumerable.Empty<Connector>().ToList();
                }

                await response.EnsureSuccessStatusCode(ApiServiceNames.ConnectorCore);
                var connectors = await response.Content.ReadAsAsync<List<Connector>>();
                return connectors;
            }
        }

        public async Task<ConnectorType> GetConnectorTypeAsync(Guid connectorTypeId)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.ConnectorCore))
            {
                var response = await client.GetAsync($"connectortypes/{connectorTypeId}");
                await response.EnsureSuccessStatusCode(ApiServiceNames.ConnectorCore);
                var connectorType = await response.Content.ReadAsAsync<ConnectorType>();
                return connectorType;
            }
        }

        public async Task<List<ConnectorType>> GetConnectorTypesAsync()
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.ConnectorCore))
            {
                var response = await client.GetAsync($"connectortypes");
                await response.EnsureSuccessStatusCode(ApiServiceNames.ConnectorCore);
                var connectorTypes = await response.Content.ReadAsAsync<List<ConnectorType>>();
                return connectorTypes;
            }
        }

        public async Task<string> GetSchemaTemplateAsync(Guid schemaId)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.ConnectorCore))
            {
                var response = await client.GetAsync($"schemas/{schemaId}/template");
                await response.EnsureSuccessStatusCode(ApiServiceNames.ConnectorCore);
                var template = await response.Content.ReadAsStringAsync();
                return template;
            }
        }

        public async Task SetConnectorEnabled(Guid connectorId, bool enabled)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.ConnectorCore))
            {
                var url = enabled ? $"connectors/{connectorId}/enable" : $"connectors/{connectorId}/disable";
                var response = await client.PostAsync(url, null);
                await response.EnsureSuccessStatusCode(ApiServiceNames.ConnectorCore);
            }
        }

        public async Task<List<Equipment>> GetSiteEquipmentsWithCategory(Guid siteId)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.ConnectorCore))
            {
                var response = await client.GetAsync($"sites/{siteId}/allEquipmentsWithCategory");
                await response.EnsureSuccessStatusCode(ApiServiceNames.ConnectorCore);
                return await response.Content.ReadAsAsync<List<Equipment>>();
            }
        }

        public async Task<ConnectorScan> CreateConnectorScanAsync(Guid connectorId, ConnectorScan connectorScan)
        {
            using var client = _httpClientFactory.CreateClient(ApiServiceNames.ConnectorCore);
            var response = await client.PostAsJsonAsync($"connectors/{connectorId}/scans", connectorScan);
            await response.EnsureSuccessStatusCode(ApiServiceNames.ConnectorCore);
            return await response.Content.ReadAsAsync<ConnectorScan>();
        }

        public async Task UpdateConnectorScanAsync(Guid connectorId,Guid scanId, UpdateConnectorScanRequest request)
        {
            using var client = _httpClientFactory.CreateClient(ApiServiceNames.ConnectorCore);
            var urlBuilder = new StringBuilder($"connectors/{connectorId}/scans/{scanId}")
                .Append($"?status={request.Status}")
                .Append($"&errorMessage={request.ErrorMessage}")
                .Append($"&errorCount={request.ErrorCount}")
                .Append($"&startTime={request.Started:yyyy-MM-dd'T'HH:mm:ss'Z'}")
                .Append($"&endTime={request.Finished:yyyy-MM-dd'T'HH:mm:ss'Z'}");
            var response = await client.PatchAsync(urlBuilder.ToString(), null);
            await response.EnsureSuccessStatusCode(ApiServiceNames.ConnectorCore);
        }

        public async Task<ConnectorScan> GetConnectorScanByIdAsync(Guid connectorId, Guid scanId)
        {
            using var client = _httpClientFactory.CreateClient(ApiServiceNames.ConnectorCore);
            var response = await client.GetAsync($"connectors/{connectorId}/scans/{scanId}");
            await response.EnsureSuccessStatusCode(ApiServiceNames.ConnectorCore);
            return await response.Content.ReadAsAsync<ConnectorScan>();
        }

        public async Task StopConnectorScanAsync(Guid connectorId, Guid scanId)
        {
            using var client = _httpClientFactory.CreateClient(ApiServiceNames.ConnectorCore);
            var response = await client.PatchAsync($"connectors/{connectorId}/scans/{scanId}/stop", null);
            await response.EnsureSuccessStatusCode(ApiServiceNames.ConnectorCore);
        }

        public async Task<List<ConnectorLogRecord>> GetLatestConnectorLogsAsync(Guid connectorId, int count, bool includeErrors, string source)
        {
            using var client = _httpClientFactory.CreateClient(ApiServiceNames.ConnectorCore);
            var uriBuilder = new StringBuilder($"connectors/{connectorId}/logs/latest?count={count}&includeErrors={includeErrors}");
            if (!string.IsNullOrWhiteSpace(source))
            {
                uriBuilder.Append($"&source={source}");
            }
            var response = await client.GetAsync(uriBuilder.ToString());
            await response.EnsureSuccessStatusCode(ApiServiceNames.ConnectorCore);
            var logs = await response.Content.ReadAsAsync<List<ConnectorLogRecord>>();
            return logs;
        }

        public async Task<List<ConnectorScan>> GetConnectorScansAsync(Guid connectorId)
        {
            using var client = _httpClientFactory.CreateClient(ApiServiceNames.ConnectorCore);
            var response = await client.GetAsync($"connectors/{connectorId}/scans");
            await response.EnsureSuccessStatusCode(ApiServiceNames.ConnectorCore);
            var connectorScans = await response.Content.ReadAsAsync<List<ConnectorScan>>();
            return connectorScans;
        }

        public async Task<LogErrorsCore> GetConnectorLogErrorsAsync(Guid connectorId, long logId)
        {
            using var client = _httpClientFactory.CreateClient(ApiServiceNames.ConnectorCore);
            var response = await client.GetAsync($"connectors/{connectorId}/logs/latest/{logId}/errors");
            await response.EnsureSuccessStatusCode(ApiServiceNames.ConnectorCore);
            var logErrors = await response.Content.ReadAsAsync<LogErrorsCore>();
            return logErrors;
        }

        public async Task<List<ConnectorLogRecord>> GetLogsForConnectorAsync(Guid connectorId, DateTime start, DateTime end)
        {
            using var client = _httpClientFactory.CreateClient(ApiServiceNames.ConnectorCore);
            var response = await client.GetAsync($"connectors/{connectorId}/logs?start={start:yyyy-MM-dd'T'HH:mm:ss'Z'}&end={end:yyyy-MM-dd'T'HH:mm:ss'Z'}&source=Connector");
            await response.EnsureSuccessStatusCode(ApiServiceNames.ConnectorCore);
            return await response.Content.ReadAsAsync<List<ConnectorLogRecord>>();
        }

        public async Task<List<Gateway>> GetGatewaysForSiteAsync(Guid siteId)
        {
            using var client = _httpClientFactory.CreateClient(ApiServiceNames.ConnectorCore);

            var response = await client.GetAsync($"sites/{siteId}/gateways");
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return new List<Gateway>();
            }

            await response.EnsureSuccessStatusCode(ApiServiceNames.ConnectorCore);
            var gateways = await response.Content.ReadAsAsync<List<Gateway>>();
            return gateways;
        }

        public async Task<List<ConnectivityStatistics>> GetGatewaysForMultiSitesAsync(List<Guid> siteIds)
        {
            using var client = _httpClientFactory.CreateClient(ApiServiceNames.ConnectorCore);

            var url = $"siteConnectivityStatistics";
            foreach(var siteId in siteIds)
            {
                url = QueryHelpers.AddQueryString(url, "siteIds", siteId.ToString());
            }
            var response = await client.GetAsync(url);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return new List<ConnectivityStatistics>();
            }

            await response.EnsureSuccessStatusCode(ApiServiceNames.ConnectorCore);
            var gateways = await response.Content.ReadAsAsync<List<ConnectivityStatistics>>();
            return gateways;
        }

        public async Task<Stream> DownloadScanResultAsStream(Guid connectorId, Guid scanId)
        {
            using var client = _httpClientFactory.CreateClient(ApiServiceNames.ConnectorCore);

            var response = await client.GetAsync($"connectors/{connectorId}/scans/{scanId}/content");
            await response.EnsureSuccessStatusCode(ApiServiceNames.ConnectorCore);

            return await response.Content.ReadAsStreamAsync();
        }

        public async Task<List<SetPointCommand>> GetSetPointCommandsAsync(Guid siteId, Guid? insightId, Guid? equipmentId)
        {
            using var client = _httpClientFactory.CreateClient(ApiServiceNames.ConnectorCore);

            var uri = $"sites/{siteId}/setpointcommands";
            if (equipmentId.HasValue)
            {
                uri += $"?equipmentId={equipmentId}";
            }

            var response = await client.GetAsync(uri);

            await response.EnsureSuccessStatusCode(ApiServiceNames.ConnectorCore);

            var output = (await response.Content.ReadAsAsync<List<SetPointCommand>>())
                .Where(c => insightId == null || c.InsightId == insightId.Value)
                .ToList();

            return output;
        }

        public async Task<SetPointCommand> CreateSetPointCommandAsync(SetPointCommand setPointCommand)
        {
            using var client = _httpClientFactory.CreateClient(ApiServiceNames.ConnectorCore);
            var response = await client.PostAsJsonAsync($"sites/{setPointCommand.SiteId}/setpointcommands", setPointCommand);
            await response.EnsureSuccessStatusCode(ApiServiceNames.ConnectorCore);
            return await response.Content.ReadAsAsync<SetPointCommand>();
        }

        public async Task<SetPointCommand> UpdateSetPointCommandAsync(SetPointCommand setPointCommand)
        {
            using var client = _httpClientFactory.CreateClient(ApiServiceNames.ConnectorCore);
            var response = await client.PutAsJsonAsync($"sites/{setPointCommand.SiteId}/setpointcommands/{setPointCommand.Id}", setPointCommand);
            await response.EnsureSuccessStatusCode(ApiServiceNames.ConnectorCore);
            return await response.Content.ReadAsAsync<SetPointCommand>();
        }

        public async Task DeleteSetPointCommandAsync(Guid siteId, Guid setPointCommandId)
        {
            using var client = _httpClientFactory.CreateClient(ApiServiceNames.ConnectorCore);
            var response = await client.DeleteAsync($"sites/{siteId}/setpointcommands/{setPointCommandId}");
            await response.EnsureSuccessStatusCode(ApiServiceNames.ConnectorCore);
        }

        public async Task<SetPointCommand> GetSetPointCommandAsync(Guid siteId, Guid setPointCommandId)
        {
            using var client = _httpClientFactory.CreateClient(ApiServiceNames.ConnectorCore);

            var uri = $"sites/{siteId}/setpointcommands/{setPointCommandId}";
            var response = await client.GetAsync(uri);

            await response.EnsureSuccessStatusCode(ApiServiceNames.ConnectorCore);
            return await response.Content.ReadAsAsync<SetPointCommand>();
        }

        public async Task<Device> GetDeviceAsync(Guid siteId, Guid deviceId)
        {
            using var client = _httpClientFactory.CreateClient(ApiServiceNames.ConnectorCore);
            var response = await client.GetAsync($"sites/{siteId}/devices/{deviceId}");
            await response.EnsureSuccessStatusCode(ApiServiceNames.ConnectorCore);
            return await response.Content.ReadAsAsync<Device>();
        }

        public async Task<List<SetPointCommandConfiguration>> GetSetPointCommandConfigurationsAsync()
        {
            using var client = _httpClientFactory.CreateClient(ApiServiceNames.ConnectorCore);

            var response = await client.GetAsync("setpointcommandconfigurations");

            await response.EnsureSuccessStatusCode(ApiServiceNames.ConnectorCore);
            return await response.Content.ReadAsAsync<List<SetPointCommandConfiguration>>();
        }

        public async Task SetConnectorArchived(Guid connectorId, bool archive)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.ConnectorCore))
            {
                var url = $"connectors/{connectorId}/archive";

                var response = await client.PostAsJsonAsync(url, archive);
                await response.EnsureSuccessStatusCode(ApiServiceNames.ConnectorCore);
            }
        }
    }
}
