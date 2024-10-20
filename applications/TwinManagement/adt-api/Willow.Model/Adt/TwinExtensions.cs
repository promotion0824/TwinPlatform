using Azure.DigitalTwins.Core;

namespace Willow.Model.Adt;

public static class TwinExtensions
{
    public static Twin ToApiTwin(this BasicDigitalTwin basicDigitalTwin)
    {
        return new Twin
        {
            Id = basicDigitalTwin.Id,
            ModelId = basicDigitalTwin.Metadata.ModelId,
            CustomProperties = basicDigitalTwin.Contents
        };
    }

    public static string? GetProperty(this EquatableBasicDigitalTwin twin, string property)
    {
        if (twin?.Contents == null || !twin.Contents.TryGetValue(property, out var contents))
        {
            return string.Empty;
        }

        return contents?.ToString();
    }

    public static BasicDigitalTwin ToBasicDigitalTwin(this Twin twin)
    {
        return new BasicDigitalTwin
        {
            Id = twin.Id,
            Metadata = new DigitalTwinMetadata { ModelId = twin.ModelId },
            Contents = twin.CustomProperties
        };
    }
}
