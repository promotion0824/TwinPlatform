using System.Threading.Tasks;
using AssetCoreTwinCreator.Import.Models;
using CsvHelper;

namespace AssetCoreTwinCreator.Import
{
    public interface IMappingImporter
    {
        MappingType MappingType { get; }
        Task PerformImportAsync(CsvReader csvReader);
    }
}
