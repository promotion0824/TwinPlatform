using Willow.Model.Adt;
using Willow.Model.Async;

namespace Willow.Model.Jobs;

/// <summary>
/// Adt To Adx Export Job Option.
/// </summary>
public record AdtToAdxExportJobOption : JobBaseOption
{
    public List<EntityType> ExportTargets { get; set; } = [];
}
