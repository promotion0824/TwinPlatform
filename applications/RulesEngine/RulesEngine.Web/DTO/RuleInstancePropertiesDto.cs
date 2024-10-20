using System.Collections.Generic;

namespace RulesEngine.Web.DTO;

/// <summary>
/// Properties that can be updated and ids of rule instances to be updated
/// </summary>
public class RuleInstancePropertiesDto
{
    /// <summary>
    /// Ids of rule instances to be updated
    /// </summary>
    public List<string> Ids { get; set; }

    /// <summary>
    /// Rule instance is disabled
    /// </summary>
    public string Disabled { get; set; }

    /// <summary>
    /// Rule instance review status
    /// </summary>
    public string ReviewStatus { get; set; }

    /// <summary>
    /// Comment to be added to rule instance
    /// </summary>
    public string Comment { get; set; }
}
