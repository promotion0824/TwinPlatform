using Willow.Model.Async;

namespace Willow.Model.Requests
{
    public class FindJobsRequest
    {
        public string? Id { get; set; }
        public string? UserEmail { get; set; }
        public AsyncJobStatus? Status { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }

        /// <summary>
        /// Pass TRUE to get all the details related to job, including error list
        /// - Collecting errors can take long time
        /// </summary>
        public bool FullDetails { get; set; }
    }
}

