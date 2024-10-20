using Willow.DataQuality.Model.Responses;
using Willow.DataQuality.Model.Validation;
using Willow.Model.Adt;
using Willow.Model.Async;

namespace Willow.TwinLifecycleManagement.Web.Services;

public interface IDataQualityService
{
	Task<RuleFileUploadResponse> UploadRuleFilesAsync(IEnumerable<IFormFile> formFiles);

	Task<GetRulesResponse> GetDataQualityRules();

	Task<IEnumerable<TwinsValidationJob>> GetDQValidationJobs(
			string id,
			AsyncJobStatus? status,
			string userId,
			DateTime? from,
			DateTime? to,
			bool fullDetails);

	Task<TwinsValidationJob> GetLatestDQValidationJob(AsyncJobStatus? status);

	Task<TwinsValidationJob> DQValidate(
		string userId,
		string[] modelIds,
		string locationId,
		bool? exactModelMatch,
		DateTimeOffset? startCheckTime,
		DateTimeOffset? endCheckTime);

	Task<Page<ValidationResults>> GetTwinDataQualityResultsByModelIds(
		string[] modelIds,
		string[] resultSources,
		Result[] resultTypes,
		int pageSize,
		string continuationToken,
		string searchString,
		string locationId
	);
}
