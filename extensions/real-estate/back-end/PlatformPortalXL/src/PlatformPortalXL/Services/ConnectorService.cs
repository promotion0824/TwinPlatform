using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Models;
using PlatformPortalXL.ServicesApi.ConnectorApi;
using PlatformPortalXL.ServicesApi.SiteApi;
using Willow.Common;
using Willow.ExceptionHandling.Exceptions;

namespace PlatformPortalXL.Services
{
    public interface IConnectorService
    {
        Task<ConnectorDto> GetConnector(Guid siteId, Guid connectorId);
        Task<List<ConnectorDto>> GetConnectors(Guid siteId);
        Task<List<Connector>> GetSiteConnectors(Guid siteId, bool includePointsCount);
        Task<List<Connector>> GetPortfolioConnectors(IEnumerable<Guid> siteIds);
        Task<List<Connector>> GetPortfolioConnectors(IEnumerable<Guid> siteIds, IEnumerable<Guid> connectorIds);
        Task<List<ConnectorDto>> MapConnectors(IEnumerable<Connector> connectors, bool includeArchivedStatus = false);
        Task<ConnectorDto> MapConnector(Connector connector, bool includeArchivedStatus = false);
    }

    public class ConnectorService : IConnectorService
    {
        private readonly IConnectorApiService _connectorApiService;
        private readonly ISiteApiService _siteApiService;

        public ConnectorService(IConnectorApiService connectorApiService, ISiteApiService siteApiService)
        {
            _connectorApiService = connectorApiService;
            _siteApiService = siteApiService;
        }

        public async Task<ConnectorDto> GetConnector(Guid siteId, Guid connectorId)
        {
            var connector = await _connectorApiService.GetConnectorById(siteId, connectorId);
            var connectorDto = ConnectorDto.MapFrom(connector);

            if (connectorDto == null)
            {
                throw new NotFoundException().WithData( new { siteId = siteId, ConnectorId = connectorId});
            }

            var logRecord = (await _connectorApiService.GetLatestConnectorLogsAsync(connector.Id, count: 1, includeErrors: true, source: "Connector")).FirstOrDefault();
            connectorDto.Status = PortfolioDashboardConnectorStatus.MapStatus(connector, logRecord, includeArchivedStatus: true);

            return connectorDto;
        }

        public async Task<List<ConnectorDto>> GetConnectors(Guid siteId)
        {
            var connectors = await _connectorApiService.GetConnectorsAsync(siteId, includePointsCount: true);

            var connectorsDto = ConnectorDto.MapFrom(connectors);

            foreach (var connector in connectors)
            {
                var logRecord = (await _connectorApiService.GetLatestConnectorLogsAsync(connector.Id, count: 1, includeErrors: true, source: "Connector")).FirstOrDefault();
                var connectorDto = connectorsDto.FirstOrDefault(x => x.Id == connector.Id);
                connectorDto.Status = PortfolioDashboardConnectorStatus.MapStatus(connector, logRecord, includeArchivedStatus: true);
            }

            return connectorsDto;
        }

        public async Task<List<Connector>> GetSiteConnectors(Guid siteId, bool includePointsCount)
        {
            return await _connectorApiService.GetConnectorsAsync(siteId, includePointsCount);
        }

        public async Task<List<Connector>> GetPortfolioConnectors(IEnumerable<Guid> siteIds)
        {
            var tasks = siteIds.Select(x => GetSiteConnectors(x, false));
            var connectors = (await Task.WhenAll(tasks)).SelectMany(x => x).ToList();

            return connectors;
        }

        public async Task<List<Connector>> GetPortfolioConnectors(IEnumerable<Guid> siteIds, IEnumerable<Guid> connectorIds)
        {
            var connectors = await GetPortfolioConnectors(siteIds);

            if (connectorIds?.Any() ?? false)
            {
                connectors = connectors.Where(x => connectorIds.Contains(x.Id)).ToList();
            }

            return connectors;
        }

        public async Task<List<ConnectorDto>> MapConnectors(IEnumerable<Connector> connectors, bool includeArchivedStatus = false)
        {
            var connectorDtos = new List<ConnectorDto>();

            foreach (var connector in connectors)
            {
                var connectorDto = await MapConnector(connector, includeArchivedStatus);

                connectorDtos.Add(connectorDto);
            }

            return connectorDtos;
        }

        public async Task<ConnectorDto> MapConnector(Connector connector, bool includeArchivedStatus = false)
        {
            var connectorDto = ConnectorDto.MapFrom(connector);

            var logRecord = (await _connectorApiService.GetLatestConnectorLogsAsync(connector.Id, count: 1, includeErrors: true, source: "Connector")).FirstOrDefault();

            connectorDto.Status = PortfolioDashboardConnectorStatus.MapStatus(connector, logRecord, includeArchivedStatus);

            return connectorDto;
        }
    }
}
