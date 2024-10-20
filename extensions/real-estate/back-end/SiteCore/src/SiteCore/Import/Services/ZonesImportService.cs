using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SiteCore.Entities;
using SiteCore.Import.Models;
using Willow.ExceptionHandling.Exceptions;

namespace SiteCore.Import.Services
{
    public class ZonesImportService : IImportService
    {
        private readonly SiteDbContext _context;
        private readonly ILogger<ZonesImportService> _logger;
        public ImportType ImportType => ImportType.Zone;

        public ZonesImportService(SiteDbContext context, ILogger<ZonesImportService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task PerformImportAsync(CsvReader csvReader)
        {
            _logger.LogInformation("Performing zones import");
            var fileZones = csvReader.GetRecords<ZoneImportDto>().ToList();

            ValidateData(fileZones);

            _logger.LogInformation($"Detected {fileZones.Count} records in file");

            foreach (var zone in fileZones.Where(z => !z.ZoneId.HasValue))
            {
                zone.ZoneId = Guid.NewGuid();
            }

            var layerGroupIds = fileZones.Select(z => z.LayerGroupId).Distinct().ToList();
            var existingLayerGroupIds = (await _context.LayerGroups.Select(lg => lg.Id).Distinct().ToListAsync()).ToHashSet();
            var nonExistingLayerGroupIds = layerGroupIds.Where(id => !existingLayerGroupIds.Contains(id)).ToList();
            if (nonExistingLayerGroupIds.Any())
            {
                throw new NotFoundException(new { LayerGroupIds = nonExistingLayerGroupIds });
            }

            var generatedZones = fileZones.Select(z => new ZoneEntity
                {
                    Id = z.ZoneId.Value,
                    LayerGroupId = z.LayerGroupId,
                    Geometry = z.Geometry,
                    Zindex = z.ZIndex
                })
                .ToList();

            var existingZones = await _context.Zones
                .Where(z => existingLayerGroupIds.Contains(z.LayerGroupId))
                .ToListAsync();
            _logger.LogInformation($"Detected {existingZones.Count} existing records in database");

            var newZones = generatedZones.Where(newZone => !existingZones.Any(exZone => exZone.Id == newZone.Id)).ToList();
            var updatingZones = existingZones.Where(exZone => generatedZones.Any(newZone => exZone.Id == newZone.Id)).ToList();

            _logger.LogInformation($"Detected {newZones.Count} records to insert, {updatingZones.Count} to update");

            foreach (var updatingZone in updatingZones)
            {
                var generatedZone = generatedZones.First(newZone => newZone.Id == updatingZone.Id);
                updatingZone.Geometry = generatedZone.Geometry;
                updatingZone.Zindex = generatedZone.Zindex;
                _context.Zones.Update(updatingZone);
            }

            await _context.Zones.AddRangeAsync(newZones);

            _logger.LogInformation("Saving changes");
            await _context.SaveChangesAsync();
        }

        private static void ValidateData(IEnumerable<ZoneImportDto> zones)
        {
            var duplicatedIdentifiers = zones
                .Where(z => z.ZoneId.HasValue)
                .GroupBy(g => g.ZoneId.Value)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicatedIdentifiers.Any())
            {
                throw new ArgumentException("Duplicated zone identifiers");
            }

            foreach (var zone in zones)
            {
                if (zone.LayerGroupId == Guid.Empty)
                {
                    throw new NotFoundException($"No filled layer group id for zone with geometry: {zone.Geometry}");
                }
                try
                {
                   JsonConvert.DeserializeObject<List<List<double>>>(zone.Geometry);
                }
                catch (Exception)
                {
                    throw new ArgumentException(
                        $"Geometry of zone for layergroup {zone.LayerGroupId} is not valid. Expected array of arrays of numbers but got: {zone.Geometry}");
                }
            }
        }
    }
}
