using System.Text.RegularExpressions;
using Willow.Model.Requests;

namespace Willow.Model.Adt;

public class ExportColumn : CustomColumnRequest
{
    private string _queryFormat = string.Empty;

    private IList<(int, string)>? _queryValues;

    public bool SkipOnTemporary { get; set; }

    public bool WriteBackToADT { get; set; }

    public string? AdtPropName { get; set; }

    public string? QueryFormat
    {
        get
        {
            if (SourceType == CustomColumnSource.Path || string.IsNullOrEmpty(Source))
                return null;

            if (string.IsNullOrEmpty(_queryFormat))
            {
                _queryFormat = Source;
                var matches = Regex.Matches(Source, @"\{(.+?)\}");
                var index = 0;

                foreach (Match match in matches)
                {
                    _queryFormat = _queryFormat.Replace(match.Value, $"{{{index}}}");
                    index++;
                }
            }
            return _queryFormat;
        }
    }

    public IEnumerable<(int, string)>? QueryPaths
    {
        get
        {
            if (SourceType == CustomColumnSource.Path || string.IsNullOrEmpty(Source))
                return null;

            if (_queryValues == null)
            {
                var matches = Regex.Matches(Source, @"\{(.+?)\}");
                _queryValues = new List<(int, string)>();
                var index = 0;

                foreach (Match match in matches)
                {
                    _queryValues.Add((index, match.Value.Replace("{", string.Empty).Replace("}", string.Empty)));
                    index++;
                }
            }
            return _queryValues;
        }
    }
}

// ExportColumn comparer based on Name property
public class ExportColumnComparer : IEqualityComparer<ExportColumn>
{
    public bool Equals(ExportColumn? x, ExportColumn? y)
    {
        // Check if both objects are null or if their IDs are equal
        return x != null && y != null && x.Name == y.Name;
    }

    public int GetHashCode(ExportColumn obj)
    {
        // Return the hash code based on the ID property
        return obj.Name?.GetHashCode() ?? 0;
    }
}
