using System.Text.Json;

namespace PlatformPortalXL.Dto
{
	public class CustomerUserTimeSeriesDto
	{
		public JsonElement State { get; set; }
		public JsonElement Favorites { get; set; }
		public JsonElement RecentAssets { get; set; }
		public JsonElement ExportedCsvs { get; set; }
	}
}
