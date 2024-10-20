using Willow.Rules.Model;

namespace RulesEngine.Web;

/// <summary>
/// Tracks execution of a rule
/// </summary>
public class RuleExecutionDto
{
    /// <summary>
    /// Creates a new <see cref="RuleExecutionDto" />
    /// </summary>
    /// <param name="r"></param>
    public RuleExecutionDto(RuleExecution r)
    {
        this.RuleId = r.RuleId;
        this.Percentage = r.Percentage;
    }

    /// <summary>
    /// Rule Id
    /// </summary>
    public string RuleId { get; set; }

    /// <summary>
    /// Percentage complete
    /// </summary>
    public double Percentage { get; set; }
}
