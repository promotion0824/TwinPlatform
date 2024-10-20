using System.Collections.Generic;

namespace DigitalTwinCore.Dto
{
    public class ClosestWithCustomPropertyQuery
    {
        /// <summary>
        /// TwinIds of the twins for which to resolve closest matching twin
        /// </summary>
        public IEnumerable<string> TwinIds { get; set; }

        /// <summary>
        /// Relationships to traverse when searching for matching twins
        /// </summary>
        public string[] Relationships { get; set; }

        /// <summary>
        /// Custom property that will deem a twin as a match when found
        /// </summary>
        public string CustomPropertyToFind { get; set; }

        /// <summary>
        /// Maximum number of hops to perform when traversing from original twin(s)
        /// </summary>
        public int MaxNumberOfHops { get; set; }
    }
}
