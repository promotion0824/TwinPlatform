using Willow.Model.Adt;

namespace Willow.Model.Adx;

public class Schema
{
    public string? Name { get; set; }

    public IEnumerable<ExportColumn> TableDefinitions { get; set; } = Array.Empty<ExportColumn>();

    public IEnumerable<MaterializedView> MaterializedViews { get; set; } = Array.Empty<MaterializedView>();

    public IEnumerable<Function> Functions { get; set; } = Array.Empty<Function>();
}


public class MaterializedView
{
    public string? Name { get; set; }
    public string? Table { get; set; }
    public string? Body { get; set; }
    public bool Backfill { get; set; }
    public bool EnforceInfiniteCachingOnTable { get; set; }
}

#pragma warning disable CA1716 // Identifiers should not match keywords
public class Function
#pragma warning restore CA1716 // Identifiers should not match keywords
{
    public string? Name { get; set; }
    public string? Folder { get; set; }
    public string? Body { get; set; }
}
