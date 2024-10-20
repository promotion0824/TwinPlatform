using Azure.DigitalTwins.Core;

namespace Willow.Model.Adt;

public class EquatableBasicDigitalTwin : BasicDigitalTwin, IEquatable<EquatableBasicDigitalTwin>
{
    public EquatableBasicDigitalTwin(BasicDigitalTwin twin)
    {
        Contents = twin.Contents;
        ETag = twin.ETag;
        Id = twin.Id;
        Metadata = twin.Metadata;
    }

    public bool Equals(EquatableBasicDigitalTwin? other)
    {
        return other != null && Id.Equals(other.Id, StringComparison.Ordinal);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not EquatableBasicDigitalTwin item)
        {
            return false;
        }

        return Id.Equals(item.Id, StringComparison.Ordinal);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}
