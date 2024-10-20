using Azure.DigitalTwins.Core;

namespace Willow.Model.Adt;

public class NestedTwin
{
    public string? ParentId { get; set; }
    public BasicDigitalTwin Twin { get; set; }
    public IList<NestedTwin> Children { get; set; } = new List<NestedTwin>();

    public NestedTwin(BasicDigitalTwin twin, string? parentId = null)
    {
        Twin = twin;
        ParentId = parentId;
    }

    public NestedTwin(BasicDigitalTwin twin, IList<NestedTwin> children, string? parentId = null)
        : this(twin, parentId)
    {
        Children = children;
    }
}
