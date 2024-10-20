using JsonDiffPatchDotNet;
using System.Text.Json;

namespace Willow.Model.Adt;

public class DigitalTwinsModelBasicData : IEquatable<DigitalTwinsModelBasicData>
{
    public string DtdlModel { get; set; } = default!;

    public string Id { get; set; } = default!;

    public DateTimeOffset? UploadedOn { get; set; }

    public bool Equals(DigitalTwinsModelBasicData? other)
    {
        return other != null && Id.Equals(other.Id, StringComparison.Ordinal);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not DigitalTwinsModelBasicData item)
        {
            return false;
        }

        return Id.Equals(item.Id, StringComparison.Ordinal);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public bool ExactMatch(DigitalTwinsModelBasicData other, IEnumerable<string>? ignore = null)
    {
        if (other == null)
            return false;

        if (!Equals(other))
            return false;

        var diffObj = new JsonDiffPatch();
        var differences = diffObj.Diff(DtdlModel, other.DtdlModel);
        if (differences == null)
            return true;

        var diffProperties = JsonSerializer.Deserialize<Dictionary<string, object>>(differences);
        if (ignore != null && ignore.Any())
            diffProperties = diffProperties?.Where(x => !ignore.Contains(x.Key)).ToDictionary(x => x.Key, x => x.Value);

        var hasDifferences = diffProperties?.Count > 0;
        return !hasDifferences;
    }
}
