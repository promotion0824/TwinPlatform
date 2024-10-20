namespace Willow.LiveData.Core.Features.Telemetry.DTOs;

internal record DataQualityResponseData
{
    public bool? IsNull { get; set; }

    public bool? IsValueOutOfRange { get; set; }

    public bool? IsInvalid { get; set; }

    public DateTime LastValidationUpdatedAt { get; init; }
}
