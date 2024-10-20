using DTDLParser;
using DTDLParser.Models;
using ExcelDataReader;
using System.Data;
using Willow.Exceptions.Exceptions;
using static Willow.TwinLifecycleManagement.Web.Helpers.ImporterConstants;

namespace Willow.TwinLifecycleManagement.Web.Helpers.Converters
{
    public class ExcelFileConverter : BaseFileConverter
    {
        public ExcelFileConverter(Stream stream, IReadOnlyDictionary<Dtmi, DTEntityInfo> modelData, string siteId) : base(stream, modelData, siteId)
        {
            _fileStreamName = FileExtension.ExcelFileName;
        }

        public ExcelFileConverter(Stream stream) : base(stream)
        {
            _fileStreamName = FileExtension.ExcelFileName;
        }

        public override DataTable ReadDataTable()
        {
            try
            {
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                using var reader = ExcelReaderFactory.CreateReader(_stream);
                var result = reader.AsDataSet(new ExcelDataSetConfiguration
                {
                    ConfigureDataTable = (_) => new ExcelDataTableConfiguration
                    {
                        UseHeaderRow = true
                    }
                });

                return result.Tables[0];
            }
            catch (Exception ex)
            {
                throw new FileContentException($"Unable to parse provided excel file", ex); ;
            }
        }
    }
}
