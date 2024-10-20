namespace Willow.Api.Logging.ApplicationInsights
{
    /// <summary>
    /// The application insights options.
    /// </summary>
    public class ApplicationInsightsOptions
    {
        /// <summary>
        /// Gets or sets the instrumentation key.
        /// </summary>
        [Obsolete("Use connection string instead")]
        public string? InstrumentationKey { get; set; } = default!;

        /// <summary>
        /// Gets or sets the connection string.
        /// </summary>
        public string? ConnectionString { get; set; } = default!;

        /// <summary>
        /// Gets or sets the cloud role name.
        /// </summary>
        public string? CloudRoleName { get; set; } = default!;

        /// <summary>
        /// Gets or sets the ignore options.
        /// </summary>
        public IgnoreOptions? Ignore { get; set; } = default!;
    }

    /// <summary>
    /// Defines the ignore options for application insights.
    /// </summary>
    public class IgnoreOptions
    {
        /// <summary>
        /// Gets or sets the types of requests to ignore.
        /// </summary>
        public IgnoreRequestOptions? Requests { get; set; } = default!;

        /// <summary>
        /// Gets or sets the types of dependencies to ignore.
        /// </summary>
        public IgnoreDependencyOptions? Dependencies { get; set; } = default!;
    }

    /// <summary>
    /// Defines the ignore request options for application insights.
    /// </summary>
    public class IgnoreRequestOptions
    {
        /// <summary>
        /// Gets or sets the names of requests to ignore.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1011:Closing square brackets should be spaced correctly", Justification = "Warning is a bug in Stylecop.")]
        public string[]? Names { get; set; } = default!;

        /// <summary>
        /// Gets or sets the paths of requests to ignore.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1011:Closing square brackets should be spaced correctly", Justification = "Warning is a bug in Stylecop.")]
        public string[]? Paths { get; set; } = default!;
    }

    /// <summary>
    /// Defines the ignore dependency options for application insights.
    /// </summary>
    public class IgnoreDependencyOptions
    {
        /// <summary>
        /// Gets or sets the names of dependencies to ignore.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1011:Closing square brackets should be spaced correctly", Justification = "Warning is a bug in Stylecop.")]
        public string[]? Names { get; set; } = default!;
    }
}
