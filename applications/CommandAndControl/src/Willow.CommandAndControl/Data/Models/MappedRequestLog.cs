namespace Willow.CommandAndControl.Data.Models;

internal class MappedRequestLog : BaseEntity
{
    public required string RequestPayload { get; set; }

    public string? ResponsePayload { get; set; }

    public HttpStatusCode? SuccessCode { get; set; }

    public bool IsSuccess { get; set; } = false;

    public string? Description { get; set; }

    public required DateTimeOffset ExecutedOn { get; set; }

    public required string ExecutedBy { get; set; }
}
