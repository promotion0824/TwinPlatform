namespace Willow.Model.Responses
{
    public class ModelResponse
    {
        public string? Id { get; set; }
        public IReadOnlyDictionary<string, string>? DisplayName { get; set; }
        public IReadOnlyDictionary<string, string>? Description { get; set; }
        public bool Decommissioned { get; set; }
        public ModelStatsResponse? TwinCount { get; set; }
        public string Model { get; set; } = null!;
        public DateTimeOffset? UploadTime { get; set; }
    }

    public record ModelStatsResponse (int Exact, int Total)
    {
    }
}
