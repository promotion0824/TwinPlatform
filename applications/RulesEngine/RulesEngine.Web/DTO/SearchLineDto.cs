using Willow.Rules.Model;

namespace Willow.Rules.Web.DTO
{
    /// <summary>
    /// One search result
    /// </summary>
    public class SearchLineDto
    {
        /// <summary>
        /// Type of search result
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The key which is globally unique, suitable for a react key
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The Id for use in a link to the thing
        /// </summary>
        public string LinkId { get; set; }

        /// <summary>
        /// Score from Azure
        /// </summary>
        public double Score { get; set; }

        /// <summary>
        /// Parent chain by locatedIn and isPartOf
        /// </summary>
        /// <remarks>
        /// This is a flattened list sorted in the ascending direction.
        /// It is suitable for filtering but not for display: use a graph query for display purposes.
        /// This is copied from a RuleInstance when the insight is created
        /// </remarks>
        public TwinLocation[] Locations { get; set; } = [];
    }

}
