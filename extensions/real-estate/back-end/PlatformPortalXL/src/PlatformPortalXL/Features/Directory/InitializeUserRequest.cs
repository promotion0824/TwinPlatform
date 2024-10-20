namespace PlatformPortalXL.Features.Directory
{
    public class OldInitializeUserRequest 
    {
        public string Token { get; set; }
        public string Password { get; set; }
    }

    public class InitializeUserRequest : EmailRequest
    {
        public string Token { get; set; }
        public string Password { get; set; }
    }
}
