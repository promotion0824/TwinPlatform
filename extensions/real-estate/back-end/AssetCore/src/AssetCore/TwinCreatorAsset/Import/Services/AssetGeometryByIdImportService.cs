using System.Collections.Generic;
using System.Globalization;
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
    public class AssetGeometryByIdImportService : IMappingImporter
    {
        private readonly MappingDbContext _context;
        private readonly ILogger<AssetGeometryByIdImportService> _logger;
        public MappingType MappingType => MappingType.GeometryById;

        public AssetGeometryByIdImportService(MappingDbContext context, ILogger<AssetGeometryByIdImportService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task PerformImportAsync(CsvReader csvReader)
        {
            _logger.LogInformation("Performing asset geometry import");
            var fileGeometries = csvReader.GetRecords<AssetGeometryByIdImportData>().ToDictionary(fg => fg.AssetRegisterId);

            ValidateData(fileGeometries.Values);

            _logger.LogInformation($"Detected {fileGeometries.Count} records in file");

            var existingGeometries = (await _context.AssetGeometries.ToListAsync()).ToDictionary(m => m.AssetRegisterId);

            _logger.LogInformation($"Detected {existingGeometries.Count} existing records in database");

            var generatedGeometries = fileGeometries.Values
                .Select(fg => new AssetGeometry
                {
                    AssetRegisterId = fg.AssetRegisterId, 
                    Geometry = string.Format(CultureInfo.InvariantCulture, "[{0:F}, {1:F}]", fg.CoordX, fg.CoordY)
                })
                .ToList();

            generatedGeometries = generatedGeometries.Where(g => g != null).ToList();

            var generatedGeometriesDict = generatedGeometries.ToDictionary(g => g.AssetRegisterId);

            _logger.LogInformation($"Generated {generatedGeometries.Count} records to process");

            var newGeometries = generatedGeometriesDict.Values.Where(newGeometry => !existingGeometries.ContainsKey(newGeometry.AssetRegisterId)).ToList();
            var updatingGeometries = generatedGeometriesDict.Values.Where(ex => existingGeometries.ContainsKey(ex.AssetRegisterId)).ToList();

            _logger.LogInformation($"Detected {newGeometries.Count} records to insert, {updatingGeometries.Count} to update");

            _context.AssetGeometries.AddRange(newGeometries);

            foreach (var updatingGeometry in updatingGeometries)
            {
                var genGeometry = generatedGeometriesDict[updatingGeometry.AssetRegisterId];
                updatingGeometry.Geometry = genGeometry.Geometry;
                _context.Update(updatingGeometry);
            }

            _logger.LogInformation("Saving changes");
            await _context.SaveChangesAsync();
        }

        private void ValidateData(IEnumerable<AssetGeometryByIdImportData> assetGeometries)
        {
            var duplicatedIdentifiers = assetGeometries.GroupBy(g => g.AssetRegisterId).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            if (duplicatedIdentifiers.Any())
            {
                throw new UnprocessableEntityException("Duplicated geometry definitions for identifiers: " + string.Join(", ", duplicatedIdentifiers), null);
            }
        }
    }
}
