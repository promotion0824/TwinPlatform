using System;

namespace PlatformPortalXL.Features.Management
{
    public class UpdateSiteUserRequest
    {
        public SitePersonType Type { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string ContactNumber { get; set; }
        public string Company { get; set; }
        public Guid? RoleId { get; set; }
        public string FullName { get; set; }
    }
}
