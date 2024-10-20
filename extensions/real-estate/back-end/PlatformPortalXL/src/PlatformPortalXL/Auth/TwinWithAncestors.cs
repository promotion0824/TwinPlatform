using System.Collections.Generic;

namespace PlatformPortalXL.Auth;

/// <summary>
/// Defines the contract for providing twin ancestor state.
/// </summary>
public interface ITwinWithAncestors
{
    string TwinId { get; }

    HashSet<string> Locations { get; }
}

public record TwinWithAncestors(string TwinId, HashSet<string> Locations) : ITwinWithAncestors;
