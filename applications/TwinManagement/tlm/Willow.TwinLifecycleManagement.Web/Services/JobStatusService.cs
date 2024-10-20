using Willow.AzureDigitalTwins.SDK.Client;

namespace Willow.TwinLifecycleManagement.Web.Services
{
    public class JobStatusService : IJobStatusService
    {
        private readonly IImportClient _importClient;
        private readonly ITimeSeriesClient _timeSeriesClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="JobStatusService"/> class.
        /// </summary>
        /// <param name="importClient">Import Client.</param>
        /// <param name="timeseriesClient">Timeseries Client.</param>
        public JobStatusService(IImportClient importClient, ITimeSeriesClient timeseriesClient)
        {
            _importClient = importClient;
            _timeSeriesClient = timeseriesClient;
        }

        public async Task<TimeSeriesImportJob> GetTimeSeriesJobStatus(string jobId)
        {
            var jobsResponse = await _timeSeriesClient.FindImportsAsync(id: jobId, userId: null, fullDetails: true);
            return jobsResponse.FirstOrDefault();
        }

        public async Task CancelTimeSeriesImportJob(string jobId, string userId)
        {
            await _timeSeriesClient.CancelImportAsync(jobId, userId);
        }

    }
}
