using System.Text.Json;

namespace DirectoryCore.Dto.Requests
{
    public class CustomerUserTimeSeriesRequest
    {
        public JsonElement State { get; set; }
        public JsonElement Favorites { get; set; }
        public JsonElement RecentAssets { get; set; }
        public JsonElement ExportedCsvs { get; set; }
    }
}
