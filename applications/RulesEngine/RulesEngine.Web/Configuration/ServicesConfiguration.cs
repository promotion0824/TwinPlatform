using WillowRules.Configuration;

namespace RulesEngine.Web;

#pragma warning disable CS8618 // Nullable fields in DTO

/// <summary>
/// Options for calling apis
/// </summary>
public class ServicesConfiguration
{
    /// <summary>
    /// Name of config section in appsettings
    /// </summary>
    public const string CONFIG = "Services";

    /// <summary>
    /// The rules engine processor
    /// </summary>
    public WillowService RulesEngineProcessor { get; set; }
}

#pragma warning restore CS8618 // Nullable fields in DTO
