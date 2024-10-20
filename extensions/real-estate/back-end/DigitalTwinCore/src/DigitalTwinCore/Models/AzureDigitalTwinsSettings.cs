using System;

namespace DigitalTwinCore.Models
{
    public class AzureDigitalTwinsSettings
    {
        public Guid? TenantId { get; set; }
        public Guid? ClientId { get; set; }
        public string ClientSecret { get; set; }
        public Uri InstanceUri { get; set; }

        public bool UsesAzureIdentity =>
            string.IsNullOrWhiteSpace(ClientSecret) || ClientSecret.StartsWith("[value from") ||
            !TenantId.HasValue || TenantId.Value == Guid.Empty || !ClientId.HasValue || ClientId.Value == Guid.Empty;

    }
}
