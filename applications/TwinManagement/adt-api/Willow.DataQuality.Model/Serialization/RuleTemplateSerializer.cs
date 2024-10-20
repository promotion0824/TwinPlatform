using System.Text.Json;
using System.Text.Json.Serialization;
using Willow.DataQuality.Model.Rules;

namespace Willow.DataQuality.Model.Serialization;

public interface IRuleTemplateSerializer
{
    RuleTemplate? Deserialize(string rule, JsonSerializerOptions? options = null);

    string Serialize(RuleTemplate rule, JsonSerializerOptions? options = null);
}

public class RuleTemplateSerializer : IRuleTemplateSerializer
{
    public RuleTemplate? Deserialize(string rule, JsonSerializerOptions? options = null)
    {
        if (string.IsNullOrEmpty(rule))
            return null;

        return JsonSerializer.Deserialize<RuleTemplate>(rule, options ?? new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }

    public string Serialize(RuleTemplate rule, JsonSerializerOptions? options = null)
    {
        return JsonSerializer.Serialize(rule, options ?? new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }
}
