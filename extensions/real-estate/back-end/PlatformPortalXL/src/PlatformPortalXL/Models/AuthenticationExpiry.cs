using System;

namespace PlatformPortalXL.Models
{
    public class AuthenticationExpiry
    {
        public Guid UserId { get; set; }
        public int ExpiresIn { get; set; }
    }
}
