using DTDLParser;
using DTDLParser.Models;
using Willow.Model.Requests;
using Willow.Model.TimeSeries;
using Willow.TwinLifecycleManagement.Web.Models;
using static Willow.TwinLifecycleManagement.Web.Helpers.ImporterConstants;

namespace Willow.TwinLifecycleManagement.Web.Helpers.Converters
{
    public static class FileConverterHelper
    {
        public static FileTwinsAndRelationships GetConvertedTwinsAndRelationships(IEnumerable<IFormFile> formFiles, string siteId, IReadOnlyDictionary<Dtmi, DTEntityInfo> models)
        {
            var convertedTwinsAndRelationships = new FileTwinsAndRelationships();

            foreach (var formFile in formFiles)
            {
                var fileConverter = GetSpecificFileConverter(formFile, true, siteId, models);
                fileConverter.GetParsedTwinsWithRelationships();

                convertedTwinsAndRelationships.Twins.AddRange(fileConverter.GetParsedTwins());
                convertedTwinsAndRelationships.Relationships.AddRange(fileConverter.GetParsedRelationships());
            }

            return convertedTwinsAndRelationships;
        }

        public static FileTwinsAndRelationships GetConvertedTwins(IEnumerable<IFormFile> formFiles, string siteId, IReadOnlyDictionary<Dtmi, DTEntityInfo> models)
        {
            var convertedTwins = new FileTwinsAndRelationships();

            foreach (var formFile in formFiles)
            {
                var fileConverter = GetSpecificFileConverter(formFile, true, siteId, models);
                convertedTwins.Twins.AddRange(fileConverter.GetParsedTwins());
            }

            return convertedTwins;
        }

        public static FileTwinsAndRelationships GetConvertedRelationships(IEnumerable<IFormFile> formFiles, string siteId, IReadOnlyDictionary<Dtmi, DTEntityInfo> models)
        {
            var convertedTwinsAndRelationships = new FileTwinsAndRelationships();

            foreach (var formFile in formFiles)
            {
                var fileConverter = GetSpecificFileConverter(formFile, true, siteId, models);
                fileConverter.GetParsedTwinsWithRelationships();

                convertedTwinsAndRelationships.Relationships.AddRange(fileConverter.GetParsedRelationships());
            }

            return convertedTwinsAndRelationships;
        }

        public static List<string> GetConvertedTwinsIds(IEnumerable<IFormFile> formFiles)
        {
            var twinIds = new List<string>();

            if (formFiles?.Any() != true)
            {
                return twinIds;
            }

            foreach (var formFile in formFiles)
            {
                var fileConverter = GetSpecificFileConverter(formFile, false);
                twinIds.AddRange(fileConverter.GetTwinIds());
            }

            return twinIds;
        }

        public static BulkDeleteRelationshipsRequest GetBulkDeleteRelationshipsRequest(IEnumerable<IFormFile> formFiles, IReadOnlyDictionary<Dtmi, DTEntityInfo> models)
        {
            var relationships = new BulkDeleteRelationshipsRequest();
            var twinIds = new List<string>();
            var relationshipIds = new List<string>();

            if (formFiles?.Any() != true)
            {
                return relationships;
            }

            foreach (var formFile in formFiles)
            {
                var fileConverter = GetSpecificFileConverter(formFile, true, null, models);
                var twinsAndRelationships = fileConverter.GetParsedTwinsWithRelationships();
                twinIds.AddRange(twinsAndRelationships.Twins.Select(x => x.Id));
                relationshipIds.AddRange(twinsAndRelationships.Relationships.Select(x => x.Id));
            }
            relationships.TwinIds = twinIds;
            relationships.RelationshipIds = relationshipIds;

            return relationships;
        }

        private static BaseFileConverter GetSpecificFileConverter(IFormFile formFile, bool hasAdditionalParameters, string siteId = null,
            IReadOnlyDictionary<Dtmi, DTEntityInfo> models = null)
        {
            var fileNameExtension = Path.GetExtension(formFile.FileName);
            var stream = formFile.OpenReadStream();

            BaseFileConverter fileConverter = null;
            if (fileNameExtension.Equals(FileExtension.CsvExtensionConstant, StringComparison.InvariantCultureIgnoreCase))
            {
                switch (hasAdditionalParameters)
                {
                    case true:
                        fileConverter = new CsvFileConverter(stream, models, siteId);
                        break;
                    case false:
                        fileConverter = new CsvFileConverter(stream);
                        break;
                }
            }
            else if (fileNameExtension.Equals(FileExtension.ExcelExtensionConstant, StringComparison.InvariantCultureIgnoreCase) ||
                fileNameExtension.Equals(FileExtension.OlderExcelExtensionConstant, StringComparison.InvariantCultureIgnoreCase))
            {
                switch (hasAdditionalParameters)
                {
                    case true:
                        fileConverter = new ExcelFileConverter(stream, models, siteId);
                        break;
                    case false:
                        fileConverter = new ExcelFileConverter(stream);
                        break;
                }
            }
            return fileConverter;
        }
    }
}
