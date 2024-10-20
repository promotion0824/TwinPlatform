// Used for IOptions
#nullable disable

namespace Willow.Rules.Configuration;

/// <summary>
/// Configuration options for git sync from standard rules library
/// </summary>
public class GitSyncOptions
{
    /// <summary>
    /// Section name in config file
    /// </summary>
    public const string CONFIG = "GitSync";

    /// <summary>
    /// PAT for standard rules library Github repository. Will be 
    /// initialized from key vault, but can be optionally overriden.
    /// </summary>
    public string PAT { get; set; } = string.Empty;

    /// <summary>
    /// Remote URI for standard rules library Github repository.
    /// </summary>
    public string GithubURI { get; set; }
    
    /// <summary>
    /// Relative path within the repository to the standard rules.
    /// </summary>
    /// <value></value>
    public string StandardRulesPath { get; set; } = string.Empty;
}