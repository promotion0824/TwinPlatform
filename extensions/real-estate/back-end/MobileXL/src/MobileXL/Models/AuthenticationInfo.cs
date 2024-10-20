namespace MobileXL.Models
{
    public class AuthenticationInfo
    {
        public string AccessToken { get; set; }
        public int ExpiresIn { get; set; }
        public string RefreshToken { get; set; }
        public CustomerUser CustomerUser { get; set; }
    }
}
