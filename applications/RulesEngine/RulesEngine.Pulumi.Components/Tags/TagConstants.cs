using System.Collections.Immutable;

namespace RulesEngine.Pulumi.Components.Tags
{
    public static class TagConstants
    {
        public static readonly ImmutableList<string> TagChangesToIgnore = ImmutableList.Create(new[] {"tags.created"});
    }
}
