using System;

namespace DigitalTwinCore.Services.AdtApi
{
    public class AdtInstanceSettings
    {
        public Guid ClientId { get; set; }
        public Guid TenantId { get; set; }
        public string ClientSecret { get; set; }
        public Uri InstanceUri { get; set; }
    }
}
