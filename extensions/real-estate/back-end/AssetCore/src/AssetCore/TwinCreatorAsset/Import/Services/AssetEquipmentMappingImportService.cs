using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AssetCoreTwinCreator.Import.Models;
using AssetCoreTwinCreator.MappingId;
using AssetCoreTwinCreator.MappingId.Models;
using CsvHelper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Willow.Infrastructure.Exceptions;

namespace AssetCoreTwinCreator.Import.Services
{
    public class AssetEquipmentMappingImportService : IMappingImporter
    {
        private readonly MappingDbContext _context;
        private readonly ILogger<SiteMappingImportService> _logger;
        public MappingType MappingType => MappingType.Equipment;


        public AssetEquipmentMappingImportService(MappingDbContext context, ILogger<SiteMappingImportService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task PerformImportAsync(CsvReader csvReader)
        {
            _logger.LogInformation("Performing asset equipment mappings import");
            var fileMappingsList = csvReader.GetRecords<AssetEquipmentMapping>().ToList();

            _logger.LogInformation($"Detected {fileMappingsList.Count} records in file");

            ValidateData(fileMappingsList);

            var fileMappings = fileMappingsList.ToDictionary(m => m.AssetRegisterId);

            var existingMappings = (await _context.AssetEquipmentMappings.ToListAsync()).ToDictionary(m => m.AssetRegisterId);

            _logger.LogInformation($"Detected {existingMappings.Count} existing mappings in database");

            var newMappings = fileMappings.Values.Where(newMapping => !existingMappings.ContainsKey(newMapping.AssetRegisterId)).ToList();
            var updatingMappings = existingMappings.Values.Where(ex => fileMappings.ContainsKey(ex.AssetRegisterId)).ToList();

            _logger.LogInformation($"Detected {newMappings.Count} mappings to insert, {updatingMappings.Count} to update");

            _context.AssetEquipmentMappings.AddRange(newMappings);

            foreach (var updatingMapping in updatingMappings)
            {
                var fileMapping = fileMappings[updatingMapping.AssetRegisterId];
                updatingMapping.EquipmentId = fileMapping.EquipmentId;
                _context.Update(updatingMapping);
            }

            _logger.LogInformation("Saving changes");
            await _context.SaveChangesAsync();
        }

        private void ValidateData(ICollection<AssetEquipmentMapping> mappings)
        {
            var duplicatedRegisterIds = mappings.GroupBy(m => m.AssetRegisterId).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            var duplicatedEquipmentIds = mappings.GroupBy(m => m.EquipmentId).Where(g => g.Count() > 1).Select(g => g.Key).ToList();

            if (duplicatedRegisterIds.Any())
            {
                throw new UnprocessableEntityException("Following register ids are duplicated in file, the import is not possible: " + string.Join(", ", duplicatedRegisterIds), null);
            }

            if (duplicatedEquipmentIds.Any())
            {
                throw new UnprocessableEntityException("Following equipment ids are duplicated in file, the import is not possible: " + string.Join(", ", duplicatedEquipmentIds), null);
            }
        }
    }
}
