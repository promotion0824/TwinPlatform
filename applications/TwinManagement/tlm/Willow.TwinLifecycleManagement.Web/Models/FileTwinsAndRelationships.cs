using Azure.DigitalTwins.Core;

namespace Willow.TwinLifecycleManagement.Web.Models
{
	public class FileTwinsAndRelationships
	{
		public List<BasicDigitalTwin> Twins { get; set; }
		public List<BasicRelationship> Relationships { get; set; }

		public FileTwinsAndRelationships()
		{
			Twins = new List<BasicDigitalTwin>();
			Relationships = new List<BasicRelationship>();
		}
	}
}
