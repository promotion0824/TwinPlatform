using System.ComponentModel.DataAnnotations;
using Willow.Rules.Model;

namespace RulesEngine.UtilsApi.DTO;

/// <summary>
/// Adds a twin to the local cache
/// </summary>
public class TwinOverrideRequest
{
	/// <summary>
	/// The source environment to retrieve data from, eg: https://fd-twin-willow-prod-shared.azurefd.net/app/weu/axa-prod/rules-engine-web
	/// </summary>
	[Required]
	public string? SourceUrl { get; init; }

	/// <summary>
	/// The bearer token for your current session
	/// </summary>
	[Required]
	public string? BearerToken { get; init; }

	/// <summary>
	/// The rule Id
	/// </summary>
	[Required]
	public string? RuleId { get; init; }
}
