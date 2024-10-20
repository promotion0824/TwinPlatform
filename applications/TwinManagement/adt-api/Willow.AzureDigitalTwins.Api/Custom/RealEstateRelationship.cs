using System;
using System.Collections.Generic;
using Azure.DigitalTwins.Core;

namespace Willow.AzureDigitalTwins.Api.Custom;

[Serializable]
public class RealEstateRelationship
{
	public string Id { get; set; }
	public string TargetId { get; set; }
	public string SourceId { get; set; }
	public string Name { get; set; }
	public IDictionary<string, object> CustomProperties { get; set; } = new Dictionary<string, object>();
	public string ExportTime { get; init; } = DateTime.UtcNow.ToString("s");

	internal static RealEstateRelationship MapFrom(BasicRelationship dto)
	{
		return new RealEstateRelationship
		{
			Id = dto.Id,
			TargetId = dto.TargetId,
			SourceId = dto.SourceId,
			Name = dto.Name,
			CustomProperties = dto.Properties
		};
	}

	public BasicRelationship ToBasicRelationship()
	{
		return new BasicRelationship
		{
			Id = Id,
			TargetId = TargetId,
			SourceId = SourceId,
			Name = Name,
			Properties = CustomProperties
		};
	}
}
