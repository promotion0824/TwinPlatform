using System;

namespace Willow.Rules.DTO;

/// <summary>
/// Progress expanding a rule
/// </summary>
public class ExpansionDetailsDto : ProgressDetailsDto
{
	public string RuleId { get; set; }

	public ExpansionDetailsDto(string ruleId, double percentage) : base()
	{
		this.RuleId = ruleId;
		this.Percentage = percentage;
	}

	public override bool Equals(ProgressDetailsDto? other)
	{
		return other is ExpansionDetailsDto ed && ed.RuleId == this.RuleId;
	}
}