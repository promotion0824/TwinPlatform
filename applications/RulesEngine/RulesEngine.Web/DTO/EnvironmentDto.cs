#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
namespace Willow.Rules.Web;

/// <summary>
/// Environment for website
/// </summary>
public class EnvironmentDto
{
	/// <summary>
	/// Redirect url
	/// </summary>
	public string Redirect { get; internal set; }

	/// <summary>
	/// Environment Id
	/// </summary>
	public string EnvironmentId { get; internal set; }

	/// <summary>
	/// Environment name
	/// </summary>
	public string EnvironmentName { get; internal set; }
}
