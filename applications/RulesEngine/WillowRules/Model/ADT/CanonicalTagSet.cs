using System;
using System.Linq;

namespace Willow.Rules.Model
{
    /// <summary>
    /// A canonical tag set has a name with the tags in a canonical order and also exposes
    /// the tag categories that were used in the canonicalization
    /// </summary>
    public class CanonicalTagSet
    {
        /// <summary>
        /// Empty tag set
        /// </summary>
        internal static readonly CanonicalTagSet Empty = new CanonicalTagSet(Array.Empty<string>(), 0);

        public string Substance { get; init; } = "";
        public string DescribesSubstance { get; init; } = "";
        public string Position { get; init; } = "";
        public string DescribesEquipment { get; init; } = "";
        public string Equipment { get; init; } = "";
        public string DescribesMeasure { get; init; } = "";
        public string Measure { get; init; } = "";
        public string Units { get; init; } = "";
        public string Value { get; init; } = "";
        public string DescribesCapability { get; init; } = "";
        public string CapabilityType { get; init; } = "";
        public string Qualifier { get; init; } = "";

        public string[] All => new[] { DescribesSubstance, Substance, Position, DescribesEquipment, Equipment,
            DescribesMeasure, Measure, Units, Value, DescribesCapability, CapabilityType, Qualifier };

        public int Count { get; }

        private CanonicalTagSet(string[] tags, int count)
        {
            // Remove just plain bad tags
            tags = tags.Except(TagSet.Ignores).ToArray();

            // Group that tags by function and put them in a canonical order

            var grouped = tags
                .SelectMany(t => TagSet.cleanTag(t))
                .GroupBy(t => TagSet.getGroup(t));

            string combine(IGrouping<int, string>? group) => string.Join(" ", group?.AsEnumerable() ?? Array.Empty<string>());

            this.Substance = combine(grouped.FirstOrDefault(g => g.Key == 0));
            this.DescribesSubstance = combine(grouped.FirstOrDefault(g => g.Key == 1));
            this.Position = combine(grouped.FirstOrDefault(g => g.Key == 2));
            this.DescribesEquipment = combine(grouped.FirstOrDefault(g => g.Key == 3));
            this.Equipment = combine(grouped.FirstOrDefault(g => g.Key == 4));
            this.DescribesMeasure = combine(grouped.FirstOrDefault(g => g.Key == 5));
            this.Measure = combine(grouped.FirstOrDefault(g => g.Key == 6));
            this.Units = combine(grouped.FirstOrDefault(g => g.Key == 7));
            this.Value = combine(grouped.FirstOrDefault(g => g.Key == 8));
            this.DescribesCapability = combine(grouped.FirstOrDefault(g => g.Key == 9));
            this.CapabilityType = combine(grouped.FirstOrDefault(g => g.Key == 10));
            this.Qualifier = combine(grouped.FirstOrDefault(g => g.Key == 11));
            Count = count;
        }

        /// <summary>
        /// Canonicalize a tag set by putting the tags in a prescribed order
        /// </summary>
        public static CanonicalTagSet Create(string tags, int count) => 
            Create(tags.Split(' ', StringSplitOptions.RemoveEmptyEntries), count);

        /// <summary>
        /// Canonicalize a tag set by putting the tags in a prescribed order
        /// </summary>
        public static CanonicalTagSet Create(string[] tags, int count)
        {
            // Group that tags by function and put them in a canonical order
            return new CanonicalTagSet(tags, count);
        }


        public override string ToString()
        {
            return string.Join(" ", All.Where(x => !string.IsNullOrWhiteSpace(x)));
        }
    }

}