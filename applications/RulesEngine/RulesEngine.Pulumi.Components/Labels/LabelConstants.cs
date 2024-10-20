using System.Collections.Immutable;

namespace RulesEngine.Pulumi.Components.Labels
{
    public static class LabelConstants
    {
        public static readonly ImmutableList<string> LabelChangesToIgnore = ImmutableList.Create(new[] {"created"});
    }
}
