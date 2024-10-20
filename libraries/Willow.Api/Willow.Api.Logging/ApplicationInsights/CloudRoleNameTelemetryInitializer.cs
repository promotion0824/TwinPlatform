namespace Willow.Api.Logging.ApplicationInsights
{
    using System.Reflection;
    using Microsoft.ApplicationInsights.Channel;
    using Microsoft.ApplicationInsights.Extensibility;

    /// <summary>
    /// The cloud role name telemetry initializer.
    /// </summary>
    public class CloudRoleNameTelemetryInitializer : ITelemetryInitializer
    {
        private readonly string? roleName;

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudRoleNameTelemetryInitializer"/> class.
        /// </summary>
        /// <param name="roleName">The role name to add to the telemetry.</param>
        public CloudRoleNameTelemetryInitializer(string? roleName)
        {
            this.roleName = roleName;
        }

        /// <summary>
        /// Initializes the telemetry to add the role name.
        /// </summary>
        /// <param name="telemetry">The telemetry entry to add metadata to.</param>
        public void Initialize(ITelemetry telemetry)
        {
            if (!string.IsNullOrWhiteSpace(roleName))
            {
                telemetry.Context.Cloud.RoleName = roleName;
            }
            else
            {
                var assemblyName = Assembly.GetEntryAssembly()?.GetName();
                telemetry.Context.Cloud.RoleName = assemblyName?.Name;
            }
        }
    }
}
