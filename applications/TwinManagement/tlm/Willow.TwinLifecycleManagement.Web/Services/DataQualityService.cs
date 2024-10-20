using Willow.AzureDigitalTwins.SDK.Client;
using Willow.DataQuality.Model.Responses;
using Willow.DataQuality.Model.Validation;
using Willow.Model.Adt;
using Willow.Model.Async;

namespace Willow.TwinLifecycleManagement.Web.Services;

public class DataQualityService : IDataQualityService
{
	private readonly IDQValidationClient _dqValidationClient;
	private readonly IDQRuleClient _dqRuleClient;

	public DataQualityService(IDQValidationClient dqValidationclient, IDQRuleClient dqRuleClient)
	{
		_dqValidationClient = dqValidationclient;
		_dqRuleClient = dqRuleClient;
	}

	public async Task<RuleFileUploadResponse> UploadRuleFilesAsync(IEnumerable<IFormFile> formFiles)
	{
		var files = formFiles.Select(f => new FileParameter(f.OpenReadStream(), f.FileName));
		return await _dqRuleClient.UploadRuleFilesAsync(files);
	}

	public async Task<GetRulesResponse> GetDataQualityRules()
	{
		return await _dqRuleClient.GetDataQualityRulesAsync();
	}

	public async Task<IEnumerable<TwinsValidationJob>> GetDQValidationJobs(string id, AsyncJobStatus? status, string userId, DateTime? from, DateTime? to, bool fullDetails)
	{
		return await _dqValidationClient.FindValidationJobsAsync(id, userId, status, from, to, fullDetails);
	}

	public async Task<TwinsValidationJob> GetLatestDQValidationJob(AsyncJobStatus? status)
	{
		try
		{
			return await _dqValidationClient.GetLatestValidationJobAsync(status);
		}
		catch (Exception ex)
		{
			var apiException = ex as ApiException;
			if (apiException.StatusCode == 404)
			{
				return null;
			}

			throw;
		}
	}

	public async Task<TwinsValidationJob> DQValidate(
		string userId,
		string[] modelIds,
		string locationId,
		bool? exactModelMatch,
		DateTimeOffset? startCheckTime = null,
		DateTimeOffset? endCheckTime = null)
	{
		return await _dqValidationClient.TriggerTwinsValidationAsync(userId, modelIds, locationId, exactModelMatch, startCheckTime, endCheckTime);
	}

	public async Task<Page<ValidationResults>> GetTwinDataQualityResultsByModelIds(
		string[] modelIds = null,
		string[] resultSources = null,
		Result[] resultTypes = null,
		int pageSize = 100,
		string continuationToken = null,
		string searchString = null,
		string locationId = null
	)
	{
		return await _dqValidationClient.GetTwinDataQualityResultsByModelIdsAsync(modelIds, resultSources, resultTypes, pageSize: pageSize, searchString: searchString, locationId: locationId, continuationToken: continuationToken);
	}
}
