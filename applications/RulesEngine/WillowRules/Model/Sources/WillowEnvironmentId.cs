using System.Diagnostics;

namespace Willow.Rules.Sources;

/// <summary>
/// Strigly-typed environment id
/// </summary>
[DebuggerDisplay("{id}")]
public class WillowEnvironmentId
{
	private readonly string id;

	/// <summary>
	/// Creates a new WillowEnvironmentId
	/// </summary>
	public WillowEnvironmentId(string id)
	{
		this.id = id;
	}

	/// <summary>
	/// Convert environment Id to string
	/// </summary>
	public static implicit operator string(WillowEnvironmentId wid)
	{
		return wid.id;
	}

	/// <inheritdoc />
	public override string ToString()
	{
		return this.id;
	}
}
