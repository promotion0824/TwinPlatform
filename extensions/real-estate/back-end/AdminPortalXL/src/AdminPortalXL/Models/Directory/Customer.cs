using System;

namespace AdminPortalXL.Models.Directory
{
    public class Customer
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Country { get; set; }
        public Guid? LogoId { get; set; }
        public int Status { get; set; }
        public string LogoPath { get; set; }
        public string AccountExternalId { get; set; }
        public string SigmaConnectionId { get; set; }

        public string RegionId { get; set; }
    }
}
