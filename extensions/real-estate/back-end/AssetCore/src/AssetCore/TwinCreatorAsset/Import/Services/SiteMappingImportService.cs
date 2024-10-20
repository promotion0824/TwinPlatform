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
    public class SiteMappingImportService : IMappingImporter
    {
        public MappingType MappingType => MappingType.Site;

        private readonly MappingDbContext _context;
        private readonly ILogger<SiteMappingImportService> _logger;

        public SiteMappingImportService(MappingDbContext context, ILogger<SiteMappingImportService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task PerformImportAsync(CsvReader csvReader)
        {
            _logger.LogInformation("Performing site mappings import");
            var fileMappings = csvReader.GetRecords<SiteMapping>().ToDictionary(m => m.SiteId);

            _logger.LogInformation($"Detected {fileMappings.Count} records in file");

            var existingMappings = (await _context.SiteMappings.ToListAsync()).ToDictionary(m => m.SiteId);

            _logger.LogInformation($"Detected {existingMappings.Count} existing mappings in database");

            var newMappings = fileMappings.Values.Where(newMapping => !existingMappings.ContainsKey(newMapping.SiteId)).ToList();
            var updatingMappings = existingMappings.Values.Where(ex => fileMappings.ContainsKey(ex.SiteId)).ToList();

            _logger.LogInformation($"Detected {newMappings.Count} mappings to insert, {updatingMappings.Count} to update");

            _context.SiteMappings.AddRange(newMappings);

            foreach (var updatingMapping in updatingMappings)
            {
                var fileMapping = fileMappings[updatingMapping.SiteId];
                updatingMapping.BuildingId = fileMapping.BuildingId;
                _context.Update(updatingMapping);
            }

            _logger.LogInformation("Saving changes");
            await _context.SaveChangesAsync();
        }

        
    }
}
