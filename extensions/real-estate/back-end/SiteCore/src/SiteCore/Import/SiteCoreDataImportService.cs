using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SiteCore.Import.Models;
using Willow.Common;

namespace SiteCore.Import
{
    public interface ISiteCoreDataImportService
    {
        Task PerformImportAsync(IFormFile formFile);
    }

    public class SiteCoreDataImportService : ISiteCoreDataImportService
    {
        private readonly ILogger<SiteCoreDataImportService> _logger;
        private readonly List<IImportService> _importers;

        public SiteCoreDataImportService(
            ILogger<SiteCoreDataImportService> logger, 
            IEnumerable<IImportService> importers)
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
            finally
            {
                File.Delete(tempFilePath);
            }
        }

        private async Task PerformImportInternalAsync(string inputFilePath)
        {
            _logger.LogInformation($"Reading file: {inputFilePath}");

            using (var reader = new StreamReader(inputFilePath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                // csv.Configuration.BadDataFound = null; Do we want to ignore bad data? 

                var headers = new HashSet<string>();
                try 
                {
                    await csv.ReadAsync();
                    csv.ReadHeader();

                    headers = csv.Context.HeaderRecord.ToHashSet();
                }
                catch (Exception ex)
                {
                    throw new ArgumentException("Can't process file").WithData(new { InputFilePath = inputFilePath, ex.Message });
                }

                ImportType mappingType = ImportType.None;
                if (headers.Contains("LayerGroupId") && 
                    headers.Contains("ZoneId") && 
                    headers.Contains("ZIndex") && 
                    headers.Contains("Geometry"))
                {
                    _logger.LogInformation($"Detected data for import zones");
                    mappingType = ImportType.Zone;
                }
                else
                {
                    throw new ArgumentException("Can't process file").WithData( new { InputFilePath = inputFilePath, Headers = headers });
                }

                var importer = _importers.FirstOrDefault(im => im.ImportType == mappingType);
                if (importer == null)
                {
                    throw new NotFoundException($"Not found importer for type [{mappingType}]").WithData(new { InputFilePath = inputFilePath, Headers = headers, MappingType = mappingType });
                }

                await importer.PerformImportAsync(csv);
            }
        }
    }
}
