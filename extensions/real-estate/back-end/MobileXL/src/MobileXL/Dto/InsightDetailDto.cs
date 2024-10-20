using MobileXL.Models;
using System;

namespace MobileXL.Dto;

public class InsightDetailDto
{
	public Guid Id { get; set; }
	public Guid CustomerId { get; set; }
	public Guid SiteId { get; set; }
	public string FloorCode { get; set; }
	public InsightType Type { get; set; }
	public string Name { get; set; }
	public string Description { get; set; }
	public int Priority { get; set; }
	public Guid? EquipmentId { get; set; }
	public string TwinId { get; set; }
	public Guid? FloorId { get; set; }
	public InsightStatus LastStatus { get; set; }
	public DateTime OccurredDate { get; set; }
	public DateTime DetectedDate { get; set; }
	public InsightSourceType SourceType { get; set; }
	public Guid? SourceId { get; set; }
	public string RuleId { get; set; }
	public string RuleName { get; set; }
	public string PrimaryModelId { get; set; }

	public static InsightDetailDto MapFromModel(Insight insight)
	{
		return new InsightDetailDto
		{
			Id = insight.Id,
			CustomerId = insight.CustomerId,
			SiteId = insight.SiteId,
			FloorCode = insight.FloorCode,
			EquipmentId = insight.EquipmentId,
			TwinId = insight.TwinId,
			Type = insight.Type,
			Name = insight.Name,
			Description = insight.Description,
			Priority = insight.Priority,
			LastStatus = insight.LastStatus,
			OccurredDate = insight.OccurredDate,
			DetectedDate = insight.DetectedDate,
			SourceType = insight.SourceType,
			SourceId = insight.SourceId,
			RuleId = insight.RuleId,
			RuleName = insight.RuleName,
			PrimaryModelId = insight.PrimaryModelId
		};
	}
}
