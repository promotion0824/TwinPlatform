using System.Text.RegularExpressions;
using Willow.DataQuality.Model.Rules;

namespace Willow.DataQuality.Execution.Parsers;

public interface IPathParser
{
    /// <summary>
    /// Takes a rule template path and return a Path object
    /// </summary>
    /// <remarks>
    /// Input example: ([this])-[:hasDocument]->([dtmi:com:willowinc:Warranty])
    /// Output example: { hops: 1, relationshipNames: ["hasDocument"], models: ["dtmi:com:willowinc:Warranty;1"]}
    /// </remarks>
    /// <param name="ruleTemplatePath"></param>
    Path GetPath(RuleTemplatePath ruleTemplatePath);
}

public class PathParser : IPathParser
{
    /// <summary>
    /// Takes a rule template path and return a Path object
    /// </summary>
    /// <remarks>
    /// Input example: ([this])-[:hasDocument]->([dtmi:com:willowinc:Warranty])
    /// Output example: { hops: 1, relationshipNames: ["hasDocument"], models: ["dtmi:com:willowinc:Warranty;1"]}
    /// </remarks>
    /// <param name="ruleTemplatePath"></param>
    public Path GetPath(RuleTemplatePath ruleTemplatePath)
    {
        var ruleValues = Regex.Matches(ruleTemplatePath.Match, @"\[(.*?)\]");

        var relationship = ruleValues[1].Groups[1].Value;
        var model = ruleValues[2].Groups[1].Value;

        return new Path
        {
            RelationshipNames = new List<string> { relationship.Split("*").First().Replace(":", "") },
            Models = new List<string> { $"{model};1" },
            Hops = relationship.Split("*").Count() > 1 ? int.Parse(relationship.Split("*").Last().Last().ToString()) : 1,
            PathExpression = ruleTemplatePath.Match
        };
    }
}

public class Path
{
    public int Hops { get; set; }

    public IEnumerable<string> RelationshipNames { get; set; } = new List<string>();

    public IEnumerable<string> Models { get; set; } = new List<string>();

    public string? PathExpression { get; set; }
}
