using System;

namespace DirectoryCore.Dto.Requests
{
    public class CreateCustomerRequest
    {
        public Guid? Id { get; set; }
        public string Name { get; set; }
        public string Country { get; set; }
        public string AccountExternalId { get; set; }
        public string SigmaConnectionId { get; set; }
    }
}
