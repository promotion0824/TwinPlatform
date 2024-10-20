using Willow.AzureDigitalTwins.SDK.Client;

namespace Willow.TwinLifecycleManagement.Web.Services
{
    /// <summary>
    /// A service for getting job status details
    /// </summary>
    public interface IJobStatusService
    {

        /// <summary>
		/// Getting time series job status details for a single job ID
		/// </summary>
        Task<TimeSeriesImportJob> GetTimeSeriesJobStatus(string jobId);

        /// <summary>
        /// Cancelling job status for a single job ID
        /// </summary>
        /// <param name="jobId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task CancelTimeSeriesImportJob(string jobId, string userId);
    }
}
