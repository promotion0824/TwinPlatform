using Willow.Platform.Users;

namespace PlatformPortalXL.Models
{
    public class AuthenticationInfo
    {
        public string AccessToken { get; set; }
        public int ExpiresIn { get; set; }
        public User CustomerUser { get; set; }
        public string RefreshToken { get; set; }
    }
}
