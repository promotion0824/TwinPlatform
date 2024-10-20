using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Willow.Rules.Cache;
using Willow.Rules.Model;

// POCO class
#nullable disable

namespace RulesEngine.Web;

/// <summary>
/// Global variable Dto
/// </summary>
public class GlobalVariableDto
{
    /// <summary>
    /// Primary key for <see cref="GlobalVariable"/> table
    /// </summary>
    public string Id { get; init; }

    /// <summary>
    /// Name of variable
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// Description for built-in variables
    /// </summary>
    public string Description { get; init; }

    /// <summary>
    /// Expresssion for the variable
    /// </summary>
    public RuleParameterDto[] Expression { get; init; } = [];

    /// <summary>
    /// An optional units for the return value
    /// </summary>
    public string Units { get; init; }

    /// <summary>
    /// Is a built-in variable
    /// </summary>
    public bool IsBuiltIn { get; init; }

    /// <summary>
    /// The type of globabl variable, eg Variable, Macro or Function
    /// </summary>
    public GlobalVariableType VariableType { get; init; }

    /// <summary>
    /// The parameters for macros and functions
    /// </summary>
    public FunctionParameterDto[] Parameters { get; init; } = [];

    /// <summary>
    /// Tags
    /// </summary>
    public string[] Tags { get; init; }

    /// <summary>
    /// Json serialized version for export / import
    /// </summary>
    public string Json { get; init; }

    /// <summary>
    /// Optionally polcy decisions
    /// </summary>
    public AuthenticatedUserAndPolicyDecisionsDto Policies { get; init; }

    /// <summary>
    /// Creates a new <see cref="GlobalVariableDto" />
    /// </summary>
    public GlobalVariableDto(GlobalVariable global, bool canViewGlobal, AuthenticatedUserAndPolicyDecisionsDto policies = null)
    {
        this.Id = global.Id;
        this.Name = global.Name;
        this.Description = global.Description;
        this.Policies = policies;

        if (canViewGlobal)
        {
            this.Expression = global.Expression.Select(v => new RuleParameterDto(v)).ToArray();
            this.Json = JsonConvert.SerializeObject(global, jsonSettings);
        }
        this.IsBuiltIn = global.IsBuiltIn;
        this.Units = global.Units;
        this.VariableType = global.VariableType;
        this.Parameters = global.Parameters.Select(v => new FunctionParameterDto(v)).ToArray();
        this.Tags = global.Tags?.ToArray();
    }

    /// <summary>
    /// Constuctor for deserialization
    /// </summary>
    public GlobalVariableDto()
    {
    }

    private static readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings
    {
        Converters = new List<JsonConverter> { new TokenExpressionJsonConverter() },
        Formatting = Formatting.Indented,
        NullValueHandling = NullValueHandling.Ignore,
        TypeNameHandling = TypeNameHandling.Auto
    };
}
