using System;
using System.Linq;
using System.Threading.Tasks;
using AssetCoreTwinCreator.Import.Models;
using AssetCoreTwinCreator.MappingId;
using AssetCoreTwinCreator.MappingId.Models;
using CsvHelper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AssetCoreTwinCreator.Import.Services
{
    public class FloorMappingImportService : IMappingImporter
    {
        private readonly MappingDbContext _context;
        private readonly ILogger<SiteMappingImportService> _logger;
        public MappingType MappingType => MappingType.Floor;

        public FloorMappingImportService(MappingDbContext context, ILogger<SiteMappingImportService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task PerformImportAsync(CsvReader csvReader)
        {
            _logger.LogInformation("Performing floor mappings import");
            var fileMappings = csvReader.GetRecords<FloorMapping>().ToDictionary(m => m.FloorId);

            _logger.LogInformation($"Detected {fileMappings.Count} records in file");

            var existingMappings = (await _context.FloorMappings.ToListAsync()).ToDictionary(m => m.FloorId);

            _logger.LogInformation($"Detected {existingMappings.Count} existing mappings in database");

            var newMappings = fileMappings.Values.Where(newMapping => !existingMappings.ContainsKey(newMapping.FloorId)).ToList();
            var updatingMappings = existingMappings.Values.Where(ex => fileMappings.ContainsKey(ex.FloorId)).ToList();

            _logger.LogInformation($"Detected {newMappings.Count} mappings to insert, {updatingMappings.Count} to update");

            _context.FloorMappings.AddRange(newMappings);

            foreach (var updatingMapping in updatingMappings)
            {
                var fileMapping = fileMappings[updatingMapping.FloorId];
                updatingMapping.BuildingId = fileMapping.BuildingId;
                updatingMapping.FloorCode = fileMapping.FloorCode;
                _context.Update(updatingMapping);
            }

            _logger.LogInformation("Saving changes");
            await _context.SaveChangesAsync();
        }
    }
}
