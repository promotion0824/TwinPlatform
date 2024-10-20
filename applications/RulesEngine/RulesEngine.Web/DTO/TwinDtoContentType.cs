namespace RulesEngine.Web.DTO;

/// <summary>
/// A twin content entry type
/// </summary>
public class TwinDtoContentType
{
    /// <summary>
    /// The content type name
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// Indicator whether content type is a bool
    /// </summary>
    public bool IsBool { get; init; }

    /// <summary>
    /// Indicator whether content type is a number
    /// </summary>
    public bool IsNumber { get; init; }

    /// <summary>
    /// Indicator whether content type is a string
    /// </summary>
    public bool IsString { get; init; }
}
