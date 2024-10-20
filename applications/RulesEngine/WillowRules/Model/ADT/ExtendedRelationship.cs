using Azure.DigitalTwins.Core;

// Poco classes
#nullable disable

namespace Willow.Rules.Model
{
	/// <summary>
	/// Extended relationship beyond the ADT basic relationship
	/// </summary>
	public class ExtendedRelationship : BasicRelationship
	{
		/// <summary>
		/// The substance: air, water, ...
		/// </summary>
		public string substance { get; set; }
	}
}

