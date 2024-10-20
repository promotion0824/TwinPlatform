using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SiteCore.Domain;
using SiteCore.Entities;
using SiteCore.Requests;
using Willow.ExceptionHandling.Exceptions;

namespace SiteCore.Services
{
    public interface ILayerGroupsService
    {
        Task<List<LayerGroup>> GetLayerGroupsAsync(Guid floorId);
        Task<LayerGroup> GetLayerGroupAsync(Guid floorId, Guid layerGroupId);
        Task<LayerGroup> CreateLayerGroupAsync(Guid floorId, CreateLayerGroupRequest createRequest);
        Task<LayerGroup> UpdateLayerGroupAsync(Guid floorId, Guid layerGroupId, UpdateLayerGroupRequest updateRequest);
        Task DeleteLayerGroupAsync(Guid floorId, Guid layerGroupId);
    }

    public class LayerGroupsService : ILayerGroupsService
    {
        private readonly SiteDbContext _context;

        public LayerGroupsService(SiteDbContext context)
        {
            _context = context;
        }

        public async Task<List<LayerGroup>> GetLayerGroupsAsync(Guid floorId)
        {
            var layerGroups = await _context.LayerGroups
                .Include(g => g.Floor)
                .Include(g => g.Layers)
                .Include(g => g.Zones)
                .Include(g => g.LayerEquipments)
                .Where(g => g.FloorId == floorId)
                .OrderBy(g => g.CreatedOn)
                .ToListAsync();

            return LayerGroupEntity.MapToDomainObjects(layerGroups);
        }

        public async Task<LayerGroup> GetLayerGroupAsync(Guid floorId, Guid layerGroupId)
        {
            var layerGroup = await GetLayerGroupEntityAsync(floorId, layerGroupId);
            if (layerGroup == null)
            {
                throw new NotFoundException(new { FloorId = floorId, LayerGroupId = layerGroupId });
            }

            return LayerGroupEntity.MapToDomainObject(layerGroup);
        }

        private async Task<LayerGroupEntity> GetLayerGroupEntityAsync(Guid floorId, Guid layerGroupId)
        {
            return await _context.LayerGroups
                .Include(g => g.Floor)
                .Include(g => g.Layers)
                .Include(g => g.Zones)
                .Include(g => g.LayerEquipments)
                .Where(g => g.Id == layerGroupId && g.FloorId == floorId)
                .FirstOrDefaultAsync();
        }

        public async Task DeleteLayerGroupAsync(Guid floorId, Guid layerGroupId)
        {
            var layerGroup = await GetLayerGroupEntityAsync(floorId, layerGroupId);
            if (layerGroup == null)
            {
                throw new NotFoundException(new { FloorId = floorId, LayerGroupId = layerGroupId });
            }

            _context.RemoveRange(layerGroup.Layers);
            _context.RemoveRange(layerGroup.LayerEquipments);
            _context.RemoveRange(layerGroup.Zones);
            _context.Remove(layerGroup);

            await _context.SaveChangesAsync();
        }

        public async Task<LayerGroup> CreateLayerGroupAsync(Guid floorId, CreateLayerGroupRequest createRequest)
        {
            var createLayerGroup = new LayerGroupEntity
            {
                Id = Guid.NewGuid(),
                FloorId = floorId,
                Name = createRequest.Name,
                Is3D = createRequest.Is3D,
                CreatedOn = DateTime.UtcNow
            };

            var createZoneItems = createRequest.Zones.Select(crz => new
            {
                ZoneEntity = new ZoneEntity
                {
                    LayerGroupId = createLayerGroup.Id,
                    Geometry = JsonConvert.SerializeObject(crz.Geometry),
                    Id = Guid.NewGuid()
                },
                EquipmentIds = crz.EquipmentIds
            })
                .ToList();

            var equipmentToZoneIdsMap = createZoneItems
                .SelectMany(zi => zi.EquipmentIds.Select(ei => new { EquipmentId = ei, ZoneId = zi.ZoneEntity.Id }))
                .ToDictionary(z => z.EquipmentId, z => z.ZoneId);

            var createZones = createZoneItems.Select(zi => zi.ZoneEntity).ToList();

            var createLayers = createRequest.Layers.Select(crl => new LayerEntity
            {
                Id = Guid.NewGuid(),
                LayerGroupId = createLayerGroup.Id,
                Name = crl.Name,
                TagName = crl.TagName
            });

            var createEquipments = createRequest.Equipments.Select(cre => new LayerEquipmentEntity
            {
                LayerGroupId = createLayerGroup.Id,
                EquipmentId = cre.Id,
                Geometry = JsonConvert.SerializeObject(cre.Geometry),
                ZoneId = equipmentToZoneIdsMap.TryGetValue(cre.Id, out var zoneId) ? zoneId : (Guid?)null
            });

            await _context.LayerGroups.AddAsync(createLayerGroup);
            await _context.Zones.AddRangeAsync(createZones);
            await _context.Layers.AddRangeAsync(createLayers);
            await _context.LayerEquipments.AddRangeAsync(createEquipments);

            await _context.SaveChangesAsync();

            return await GetLayerGroupAsync(floorId, createLayerGroup.Id);
        }

        public async Task<LayerGroup> UpdateLayerGroupAsync(
            Guid floorId, 
            Guid layerGroupId, 
            UpdateLayerGroupRequest updateRequest)
        {
            var existingLayerGroup = await GetLayerGroupEntityAsync(floorId, layerGroupId);
            if (existingLayerGroup == null)
            {
                throw new NotFoundException(new { FloorId = floorId, LayerGroupId = layerGroupId });
            }

            existingLayerGroup.Name = updateRequest.Name;
            existingLayerGroup.Is3D = updateRequest.Is3D;

            var updatingLayers = new List<LayerEntity>();
            var updatingZones = new List<ZoneEntity>();
            var updatingEquipments = new List<LayerEquipmentEntity>();

            var deletingLayers = existingLayerGroup.Layers
                .Where(exl => !updateRequest.Layers.Any(updl => updl.Id == exl.Id))
                .ToList();
            var creatingLayers = updateRequest.Layers.Where(updl =>
                    (!updl.Id.HasValue || !existingLayerGroup.Layers.Any(exl => exl.Id == updl.Id)))
                .Select(updl => new LayerEntity
                {
                    Id = updl.Id ?? Guid.NewGuid(),
                    LayerGroupId = layerGroupId,
                    Name = updl.Name,
                    TagName = updl.TagName
                })
                .ToList();

            foreach (var existingLayer in existingLayerGroup.Layers)
            {
                var updatingLayer = updateRequest.Layers.FirstOrDefault(updl => updl.Id == existingLayer.Id);
                if (updatingLayer == null)
                {
                    continue;
                }

                existingLayer.Name = updatingLayer.Name;
                existingLayer.TagName = updatingLayer.TagName;
                updatingLayers.Add(existingLayer);
            }

            var deletingZones = existingLayerGroup.Zones
                .Where(exz => !updateRequest.Zones
                .Any(updz => updz.Id == exz.Id))
                .ToList();

            var creatingZoneItems = updateRequest.Zones
                .Where(updz => (!existingLayerGroup.Zones.Any(exz => exz.Id == updz.Id)))
                .Select(updz => new
                {
                    ZoneEntity = new ZoneEntity
                    {
                        LayerGroupId = layerGroupId,
                        Geometry = JsonConvert.SerializeObject(updz.Geometry),
                        Id = updz.Id ?? Guid.NewGuid()
                    },
                    EquipmentIds = updz.EquipmentIds
                })
                .ToList();

            var creatingZones = creatingZoneItems.Select(zi => zi.ZoneEntity).ToList();

            foreach (var existingZone in existingLayerGroup.Zones)
            {
                var updatingZone = updateRequest.Zones.FirstOrDefault(updz => updz.Id == existingZone.Id);
                if (updatingZone == null)
                {
                    continue;
                }

                existingZone.Geometry = JsonConvert.SerializeObject(updatingZone.Geometry);
                updatingZones.Add(existingZone);
            }

            var equipmentsToZoneIdsMap = updateRequest.Zones.SelectMany(z => z.EquipmentIds.Select(e => new { EquipmentId = e, ZoneId = z.Id.Value }))
                .Union(creatingZoneItems.SelectMany(zi =>
                    zi.EquipmentIds.Select(e => new { EquipmentId = e, ZoneId = zi.ZoneEntity.Id })))
                .ToDictionary(zi => zi.EquipmentId, zi => zi.ZoneId);

            var deletingEquipments = existingLayerGroup
                .LayerEquipments
                .Where(exeq => !updateRequest.Equipments.Any(updeq => updeq.Id == exeq.EquipmentId && exeq.LayerGroupId == layerGroupId))
                .ToList();

            var creatingEquipments = updateRequest.Equipments.Where(updeq =>
                    (!existingLayerGroup.LayerEquipments.Any(exeq =>
                        exeq.EquipmentId == updeq.Id && exeq.LayerGroupId == layerGroupId)))
                .Select(updeq => new LayerEquipmentEntity
                {
                    LayerGroupId = layerGroupId,
                    Geometry = JsonConvert.SerializeObject(updeq.Geometry),
                    ZoneId = equipmentsToZoneIdsMap.TryGetValue(updeq.Id, out var zoneId) ? zoneId : (Guid?)null,
                    EquipmentId = updeq.Id
                })
                .ToList();

            foreach (var existingEquipment in existingLayerGroup.LayerEquipments)
            {
                var updatingEquipment = updateRequest.Equipments
                    .FirstOrDefault(updeq => updeq.Id == existingEquipment.EquipmentId && 
                                            existingEquipment.LayerGroupId == layerGroupId);
                if (updatingEquipment == null)
                {
                    continue;
                }

                existingEquipment.Geometry = JsonConvert.SerializeObject(updatingEquipment.Geometry);
                existingEquipment.ZoneId = equipmentsToZoneIdsMap.TryGetValue(updatingEquipment.Id, out var zoneId) ? zoneId : (Guid?)null;
                updatingEquipments.Add(existingEquipment);
            }

            _context.LayerGroups.Update(existingLayerGroup);
            _context.Zones.RemoveRange(deletingZones);
            _context.Zones.UpdateRange(updatingZones);
            _context.Zones.AddRange(creatingZones);
            _context.Layers.RemoveRange(deletingLayers);
            _context.Layers.UpdateRange(updatingLayers);
            _context.Layers.AddRange(creatingLayers);
            _context.LayerEquipments.RemoveRange(deletingEquipments);
            _context.LayerEquipments.UpdateRange(updatingEquipments);
            _context.LayerEquipments.AddRange(creatingEquipments);

            await _context.SaveChangesAsync();

            return await GetLayerGroupAsync(floorId, layerGroupId);
        }
    }
}
