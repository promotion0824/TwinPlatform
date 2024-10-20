namespace Willow.Rules.Model;

/// <summary>
/// A flattened version of the system graph suitable for searching
/// </summary>
public struct AncestryDto
{
	public TwinNodeDto[] Locations { get; set; }

	public TwinNodeDto[] Feeds { get; set; }

	public TwinNodeDto[] FedBy { get; set; }
}
