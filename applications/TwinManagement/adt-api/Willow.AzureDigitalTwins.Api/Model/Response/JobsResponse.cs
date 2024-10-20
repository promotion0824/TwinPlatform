using System.Collections.Generic;
using Willow.AzureDigitalTwins.Api.Persistence.Models.TwinsApi;

namespace Willow.AzureDigitalTwins.Api.Model.Response
{
    public class JobsResponse
    {
        /// <summary>
        /// Total number of jobs based on search criteria
        ///</summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// List of jobs based on search criteria
        /// </summary>
        public IAsyncEnumerable<JobsEntry> Jobs { get; set; }
    }
}
