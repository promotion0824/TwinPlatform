using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AssetCoreTwinCreator.Domain;
using AssetCoreTwinCreator.Import.Models;
using AssetCoreTwinCreator.MappingId;
using AssetCoreTwinCreator.MappingId.Models;
using CsvHelper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Willow.Infrastructure.Exceptions;

namespace AssetCoreTwinCreator.Import.Services
{
    public class AssetGeometryImportService : IMappingImporter
    {
        private readonly MappingDbContext _context;
        private readonly AssetDbContext _assetContext;
        private readonly ILogger<AssetGeometryImportService> _logger;

        public MappingType MappingType => MappingType.Geometry;

        public AssetGeometryImportService(MappingDbContext context, AssetDbContext assetContext, ILogger<AssetGeometryImportService> logger)
        {
            _context = context;
            _assetContext = assetContext;
            _logger = logger;
        }

        public async Task PerformImportAsync(CsvReader csvReader)
        {
            _logger.LogInformation("Performing asset geometry import");
            var fileGeometries = csvReader.GetRecords<AssetGeometryImportData>().ToDictionary(fg => fg.Identifier);

            ValidateData(fileGeometries.Values);

            _logger.LogInformation($"Detected {fileGeometries.Count} records in file");

            var existingGeometries = (await _context.AssetGeometries.ToListAsync()).ToDictionary(m => m.AssetRegisterId);

            _logger.LogInformation($"Detected {existingGeometries.Count} existing records in database");

            var identifiers = fileGeometries.Values.Select(g => g.Identifier).Distinct().ToList();
            var assets = await _assetContext.Assets.Where(a => identifiers.Contains(a.Identifier)).ToListAsync();

            var generatedGeometries = assets.Select(asset =>
            {
                if (!fileGeometries.TryGetValue(asset.Identifier, out var fileGeometry))
                {
                    return null;
                }

                return new AssetGeometry {AssetRegisterId = asset.Id, Geometry = fileGeometry.Geometry};
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

        private void ValidateData(ICollection<AssetGeometryImportData> assetGeometries)
        {
            var duplicatedIdentifiers = assetGeometries.GroupBy(g => g.Identifier).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            if (duplicatedIdentifiers.Any())
            {
                throw new UnprocessableEntityException("Duplicated geometry definitions for identifiers: " + string.Join(", ", duplicatedIdentifiers), null);
            }

            foreach (var assetGeometry in assetGeometries)
            {
                try
                {
                    JsonConvert.DeserializeObject<List<double>>(assetGeometry.Geometry);
                }
                catch (Exception e)
                {
                    throw new UnprocessableEntityException($"Geometry for asset {assetGeometry.Identifier} is not valid. Expected array of numbers but got: {assetGeometry.Geometry}", null, e);
                }
            }
        }
    }
}
