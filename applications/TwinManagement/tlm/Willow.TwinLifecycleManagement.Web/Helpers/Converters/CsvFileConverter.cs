using CsvHelper;
using DTDLParser;
using DTDLParser.Models;
using System.Data;
using System.Globalization;
using Willow.Exceptions.Exceptions;
using static Willow.TwinLifecycleManagement.Web.Helpers.ImporterConstants;

namespace Willow.TwinLifecycleManagement.Web.Helpers.Converters
{
    public class CsvFileConverter : BaseFileConverter
    {
        public CsvFileConverter(Stream stream, IReadOnlyDictionary<Dtmi, DTEntityInfo> modelData, string siteId) : base(stream, modelData, siteId)
        {
            _fileStreamName = FileExtension.CsvFileName;
        }

        public CsvFileConverter(Stream stream) : base(stream)
        {
            _fileStreamName = FileExtension.CsvFileName;
        }

        public override DataTable ReadDataTable()
        {
            try
            {
                using StreamReader streamReader = new StreamReader(_stream);
                var reader = new CsvReader(streamReader, CultureInfo.InvariantCulture);
                var dataReader = new CsvDataReader(reader);
                var output = new DataTable();
                output.Load(dataReader);

                return output;
            }
            catch (Exception ex)
            {
                throw new FileContentException($"Unable to parse provided csv file", ex);
            }
        }
    }
}
