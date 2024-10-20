using System.Collections.Generic;
using Azure.DigitalTwins.Core;
using DigitalTwinCore.Models;

namespace DigitalTwinCore.DTO.Adx
{
	public class AdxDetailedRelationshipsPacked
	{
		public IEnumerable<AdxDetailedRelationshipPacked> Relationships { get; set; }
		public IEnumerable<object> Points { get; set; }
	}

	public class AdxDetailedRelationshipPacked
	{
		public object Rel { get; set; }
		public object Target { get; set; }
	}
}
