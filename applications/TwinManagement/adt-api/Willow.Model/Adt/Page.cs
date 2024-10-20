namespace Willow.Model.Adt;

public class Page<T>
{
    public IEnumerable<T> Content { get; set; } = Array.Empty<T>();

    public string? ContinuationToken { get; set; }
}
