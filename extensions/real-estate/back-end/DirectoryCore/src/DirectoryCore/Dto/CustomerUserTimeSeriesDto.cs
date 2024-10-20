using System.Text.Json;

namespace DirectoryCore.Dto
{
    public class CustomerUserTimeSeriesDto
    {
        //This stores the time series state and it's json form is {}
        public JsonElement State { get; set; }

        //This stores the time machine favorites and it's json form is []
        public JsonElement Favorites { get; set; }

        //This stores the recent assets and it's json form is {}
        public JsonElement RecentAssets { get; set; }

        //This stores the time machine exported csvs and it's json form is []
        public JsonElement ExportedCsvs { get; set; }
    }
}
