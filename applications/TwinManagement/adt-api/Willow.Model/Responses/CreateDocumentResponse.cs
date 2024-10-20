namespace Willow.Model.Responses
{
    public class CreateDocumentResponse
    {
        public string? FileName { get; set; }

        public bool IsSuccessful { get; set; }

        public string? TwinId { get; set; }

        public string? ErrorMessage { get; set; }
    }
}
