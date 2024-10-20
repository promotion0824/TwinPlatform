using System.Threading.Tasks;
using CsvHelper;
using SiteCore.Import.Models;

namespace SiteCore.Import
{
    public interface IImportService
    {
        ImportType ImportType { get; }
        Task PerformImportAsync(CsvReader csvReader);
    }
}
