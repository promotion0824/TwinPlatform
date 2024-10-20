namespace ConnectorCore.Models
{
    using System.Collections.Generic;

    internal class HealthcheckOptions
    {
        public List<string> ExcludeSources { get; set; } = new List<string>();

        public int OfflineThresholdMinutes { get; set; } = 15;
    }
}
