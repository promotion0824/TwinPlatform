namespace Willow.Model.Responses
{
    public class AdtBulkImportJobResponse
    {
        public string? JobId { get; set; }

        public AsyncJobDetailsResponse? Details { get; set; }

        public IEnumerable<string>? Target { get; set; }

        public IDictionary<string, string>? EntitiesError { get; set; }

        public int? TotalEntities { get; set; }

        public int? ProcessedEntities { get; set; }

        public DateTime? CreateTime { get; set; }

        public string? UserEmail { get; set; }

        public string? UserData { get; set; }

        public IDictionary<string, int>? TwinsByModel { get; set; }

        public IEnumerable<string>? EntitiesId { get; set; }

    }

    public class AsyncJobDetailsResponse
    {
        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public string? Status { get; set; }

        public string? StatusMessage { get; set; }
    }
}
