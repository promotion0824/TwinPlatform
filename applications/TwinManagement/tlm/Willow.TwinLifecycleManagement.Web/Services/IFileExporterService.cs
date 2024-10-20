namespace Willow.TwinLifecycleManagement.Web.Services
{
	public interface IFileExporterService
	{
		Task<byte[]> ExportZippedTwinsAsync(string locationId, string[] modelIds, bool? exactModelMatch, bool? includeRelationships, bool? includeIncomingRelationships, bool? isTemplateExportOnly);
		Task<byte[]> ExportZippedTwinsByTwinIdsAsync(string[] twinIds);
	}
}
