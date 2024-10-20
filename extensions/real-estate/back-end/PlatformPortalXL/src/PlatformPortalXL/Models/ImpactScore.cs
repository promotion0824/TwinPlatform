
namespace PlatformPortalXL.Models
{
    public class ImpactScore
    {
	    /// <summary>
	    /// The field id of the impact score
	    /// </summary>
	    public string FieldId { get; set; }
		public string Name { get; set; }
        public double Value { get; set; }
        public string Unit { get; set; }
        /// <summary>
        /// It is an Id that matches a timeseries in ADX's ExternalId
        /// </summary>
        public string ExternalId { get; set; }
    }
}
