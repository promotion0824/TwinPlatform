namespace Willow.TwinLifecycleManagement.Web.Options
{
    /// <summary>
    /// Configurations for MTI.
    /// </summary>
    public class MtiOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether or not to display MTI sync buttons.
        /// </summary>
        public bool EnableSyncToMapped { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether or not to disable MTI health check, and approve and accept feature in UI.
        /// </summary>
        public bool IsMappedDisabled { get; set; } = false;
    }
}
