using System;

namespace AdminPortalXL.Features.Directory
{
    public class CreateCustomerRequest
    {
        public string CustomerName { get; set; }
        public string Country { get; set; }
        public string RegionId { get; set; }
        public string AccountExternalId { get; set; }
        public string SigmaConnectionId { get; set; }
    }
}
