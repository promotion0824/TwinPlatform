using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AssetCoreTwinCreator.Import.Models;
using CsvHelper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Willow.Infrastructure.Exceptions;

namespace AssetCoreTwinCreator.Import
{
    public interface IAssetMappingImportService
    {
        Task PerformImportAsync(IFormFile formFile);
    }

    public class AssetMappingImportService : IAssetMappingImportService
    {
        private readonly ILogger<AssetMappingImportService> _logger;
        private readonly List<IMappingImporter> _importers;

        public AssetMappingImportService(ILogger<AssetMappingImportService> logger, IEnumerable<IMappingImporter> importers)
        {
            _logger = logger;
            _importers = importers.ToList();
        }

        public async Task PerformImportAsync(IFormFile formFile)
        {
            var tempFilePath = Path.GetTempFileName();
            using (var inputStream = formFile.OpenReadStream())
            using (var fileStream = File.OpenWrite(tempFilePath))
            {
                await inputStream.CopyToAsync(fileStream);
            }

            try
            {
                await PerformImportInternalAsync(tempFilePath);
            }
            catch (UnprocessableEntityException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new UnprocessableEntityException($"Couldn't process provided file: " + e.Message, null, e);
            }
            finally
            {
                File.Delete(tempFilePath);
            }
        }

        private async Task PerformImportInternalAsync(string tempFilePath)
        {
            _logger.LogInformation($"Reading file: {tempFilePath}");

            using (var reader = new StreamReader(tempFilePath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                await csv.ReadAsync();
                csv.ReadHeader();
                var headers = csv.Context.HeaderRecord.ToHashSet();

                MappingType mappingType = MappingType.None;
                if (headers.Contains("SiteId") && headers.Contains("BuildingId"))
                {
                    _logger.LogInformation($"Detected data for site-building mapping");
                    mappingType = MappingType.Site;
                    
                }
                else if (headers.Contains("FloorId") && headers.Contains("BuildingId") && headers.Contains("FloorCode"))
                {
                    _logger.LogInformation($"Detected data for floors mapping");
                    mappingType = MappingType.Floor;
                }
                else if (headers.Contains("Identifier") && headers.Contains("Geometry"))
                {
                    _logger.LogInformation($"Detected data for asset geometry");
                    mappingType = MappingType.Geometry;
                }
                else if (headers.Contains("AssetRegisterId") && headers.Contains("CoordX") && headers.Contains("CoordY"))
                {
                    _logger.LogInformation($"Detected data for asset geometry by register id");
                    mappingType = MappingType.GeometryById;
                }
                else if (headers.Contains("AssetRegisterId") && headers.Contains("EquipmentId"))
                {
                    _logger.LogInformation($"Detected data for asset equipment mapping");
                    mappingType = MappingType.Equipment;
                }
                else
                {
                    throw new UnprocessableEntityException("Can't process file with headers: " + string.Join(",", headers), null);
                }

                var importer = _importers.FirstOrDefault(im => im.MappingType == mappingType);
                if (importer == null)
                {
                    throw new UnprocessableEntityException($"Not found importer for type [{mappingType}]", null);
                }

                await importer.PerformImportAsync(csv);
            }
        }
    }
}
