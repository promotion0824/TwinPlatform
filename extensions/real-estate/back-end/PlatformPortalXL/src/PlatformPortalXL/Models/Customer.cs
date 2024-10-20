using System;

namespace PlatformPortalXL.Models
{
    public class Customer
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid? LogoId { get; set; }
        public string LogoPath { get; set; }
        public string AccountExternalId { get; set; }
        public string SigmaConnectionId { get; set; }
        public CustomerFeatures Features { get; set; }
        public string CognitiveSearchUri { get; set; }
        public string CognitiveSearchIndex { get; set; }
        public int Status { get; set; }
        public string SingleTenantUrl { get; set; }
    }
}
