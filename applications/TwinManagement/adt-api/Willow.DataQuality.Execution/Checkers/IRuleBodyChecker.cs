using Willow.AzureDigitalTwins.Services.Cache.Models;
using Willow.Model.Adt;

namespace Willow.DataQuality.Execution.Checkers;

public interface IRuleBodyChecker<T, R>
{
    Task<IEnumerable<R>> Check(TwinWithRelationships twin, IEnumerable<T> expressionRules, IEnumerable<UnitInfo>? unitInfo = null);
}
