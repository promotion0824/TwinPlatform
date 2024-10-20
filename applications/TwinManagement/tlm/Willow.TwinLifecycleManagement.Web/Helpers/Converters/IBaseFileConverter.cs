using System.Data;
using Azure.DigitalTwins.Core;
using Willow.Model.Adt;

namespace Willow.TwinLifecycleManagement.Web.Helpers.Converters
{
	public interface IBaseFileConverter
	{
		IEnumerable<BasicDigitalTwin> GetParsedTwins();
		IEnumerable<BasicRelationship> GetParsedRelationships();
		BasicDigitalTwinWithRelationships GetParsedTwinsWithRelationships();
		BasicDigitalTwinWithRelationships YieldReturnParsedData();
	}
}
