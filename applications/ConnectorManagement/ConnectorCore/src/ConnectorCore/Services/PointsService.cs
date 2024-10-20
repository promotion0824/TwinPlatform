namespace ConnectorCore.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using ConnectorCore.Common.Abstractions;
    using ConnectorCore.Common.Models;
    using ConnectorCore.Dtos;
    using ConnectorCore.Entities;
    using ConnectorCore.Models;
    using ConnectorCore.Repositories;
    using Microsoft.Extensions.Options;
    using Willow.Infrastructure.Exceptions;

    internal class PointsService : IPointsService
    {
        private readonly IPointsRepository pointsRepository;
        private readonly IDevicesRepository devicesRepository;
        private readonly IEquipmentsRepository equipmentsRepository;
        private readonly ITagsRepository tagsRepository;
        private readonly IContinuationTokenProvider<PointEntity, Guid> tokenProvider;
        private readonly AppSettings appSettings;

        public PointsService(
            IPointsRepository pointsRepository,
            IDevicesRepository devicesRepository,
            IEquipmentsRepository equipmentsRepository,
            ITagsRepository tagsRepository,
            IContinuationTokenProvider<PointEntity, Guid> tokenProvider,
            IOptions<AppSettings> appSettings)
        {
            this.pointsRepository = pointsRepository;
            this.devicesRepository = devicesRepository;
            this.equipmentsRepository = equipmentsRepository;
            this.tagsRepository = tagsRepository;
            this.tokenProvider = tokenProvider;
            this.appSettings = appSettings.Value;
        }

        public async Task<GetPointsResult> GetListBySiteIdAsync(Guid siteId, string continuationToken, int? pageSize, bool? includeEquipment)
        {
            var pageSizeFinal = pageSize > 0 ? pageSize.Value : appSettings.PointsPageSize;

            var items = await pointsRepository.GetBySiteIdAsync(siteId);
            var resultItems = items.ToList();
            var totalItemsCount = resultItems.Count();

            if (!string.IsNullOrEmpty(continuationToken))
            {
                var id = tokenProvider.ParseToken(continuationToken);
                var lastPageItem = items.FirstOrDefault(q => q.Id == id);

                if (lastPageItem == null)
                {
                    throw new BadRequestException("Item with provided token can't be found.");
                }

                var lastPageItemIndex = items.IndexOf(lastPageItem);

                resultItems = items.Skip(lastPageItemIndex + 1).ToList();
                totalItemsCount = resultItems.Count();
            }

            resultItems = resultItems.Take(pageSizeFinal).ToList();

            if (includeEquipment ?? false)
            {
                await IncludeEquipmentAsync(resultItems);
            }

            var tagsDict = await tagsRepository.GetByPointIdsAsync(resultItems.Select(q => q.EntityId));

            foreach (var item in resultItems)
            {
                if (tagsDict.TryGetValue(item.EntityId, out var tags))
                {
                    item.Tags = tags;
                }
            }

            var result = new GetPointsResult();
            result.Data = PointDto.Create(resultItems);

            if (totalItemsCount > pageSizeFinal && pageSizeFinal > 0)
            {
                result.ContinuationToken = tokenProvider.GetToken(resultItems.Last());
            }

            return result;
        }

        public async Task<GetPointsResult> GetListByEquipmentIdAsync(Guid siteId, Guid equipmentId, string continuationToken, int? pageSize, bool? includeEquipment)
        {
            var pageSizeFinal = pageSize > 0 ? pageSize.Value : appSettings.PointsPageSize;

            var items = await pointsRepository.GetBySiteIdEquipmentIdAsync(siteId, equipmentId);
            var resultItems = items.ToList();
            var totalItemsCount = resultItems.Count();

            if (!string.IsNullOrEmpty(continuationToken))
            {
                var id = tokenProvider.ParseToken(continuationToken);
                var lastPageItem = items.FirstOrDefault(q => q.Id == id);

                if (lastPageItem == null)
                {
                    throw new BadRequestException("Item with provided token can't be found.");
                }

                var lastPageItemIndex = items.IndexOf(lastPageItem);

                resultItems = items.Skip(lastPageItemIndex + 1).ToList();
                totalItemsCount = resultItems.Count();
            }

            resultItems = resultItems.Take(pageSizeFinal).ToList();

            if (includeEquipment ?? false)
            {
                await IncludeEquipmentAsync(resultItems);
            }

            var tagsDict = await tagsRepository.GetByPointIdsAsync(resultItems.Select(q => q.EntityId));

            foreach (var item in resultItems)
            {
                if (tagsDict.TryGetValue(item.EntityId, out var tags))
                {
                    item.Tags = tags;
                }
            }

            var result = new GetPointsResult();
            result.Data = PointDto.Create(resultItems);

            if (totalItemsCount > pageSizeFinal && pageSizeFinal > 0)
            {
                result.ContinuationToken = tokenProvider.GetToken(resultItems.Last());
            }

            return result;
        }

        public async Task<PointIdentifier>
            GetPointIdentifierByExternalPointIdAsync(Guid siteId, string externalPointId) =>
            await pointsRepository.GetPointIdentifierByExternalPointIdAsync(siteId, externalPointId);

        public async Task<List<PointEntity>> GetByTagNameAsync(Guid siteId, string tagName, bool? includeEquipment)
        {
            var points = await pointsRepository.GetByTagNameAsync(siteId, tagName);

            if (includeEquipment ?? false)
            {
                await IncludeEquipmentAsync(points);
            }

            return points;
        }

        private async Task IncludeEquipmentAsync(IEnumerable<PointEntity> points)
        {
            var equipmentsDict = await equipmentsRepository.GetByPointIdsAsync(points.Select(p => p.EntityId).ToList());

            foreach (var pointEntity in points)
            {
                if (equipmentsDict.TryGetValue(pointEntity.EntityId, out var equipments))
                {
                    pointEntity.Equipment = equipments;
                }
            }
        }

        public async Task<PointEntity> GetAsync(Guid pointEntityId, bool? includeDevice, bool? includeEquipment)
        {
            var point = await pointsRepository.GetPointByEntityId(pointEntityId);
            if (point == null)
            {
                return null;
            }

            if (includeDevice == true)
            {
                point.Device = await devicesRepository.GetItemAsync(point.DeviceId);
            }

            if (includeEquipment == true)
            {
                point.Equipment = await equipmentsRepository.GetByPointIdAsync(point.EntityId);
            }

            point.Tags = await tagsRepository.GetByPointIdAsync(point.EntityId);

            return point;
        }

        public async Task<PointEntity> GetByIdAsync(Guid pointId)
        {
            var point = await pointsRepository.GetPointById(pointId);
            return point;
        }

        public async Task<IEnumerable<PointEntity>> GetListAsync(Guid siteId, Guid deviceId) =>
            await pointsRepository.GetBySiteIdDeviceIdAsync(siteId, deviceId);

        public async Task<IEnumerable<PointEntity>> GetListAsync(Guid[] pointEntityIds)
        {
            var points = await pointsRepository.GetByEntityIds(pointEntityIds);
            if (!points.Any())
            {
                return points;
            }

            var tagsByPointEntityId = await tagsRepository.GetByPointIdsAsync(pointEntityIds);
            var equipmentMap = await equipmentsRepository.GetByPointIdsAsync(pointEntityIds);

            foreach (var point in points)
            {
                var equipments = equipmentMap[point.EntityId];
                point.Equipment = equipments.Select(x => new EquipmentEntity { Name = x.Name }).ToList();
                point.Tags = tagsByPointEntityId[point.EntityId];
            }

            return points;
        }

        public async Task<IEnumerable<PointEntity>> GetListByConnectorIdAsync(Guid connectorId) =>
            await pointsRepository.GetByConnectorIdAsync(connectorId);
    }
}
