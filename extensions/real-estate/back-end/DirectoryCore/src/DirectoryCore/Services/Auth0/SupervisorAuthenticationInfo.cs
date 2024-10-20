using DirectoryCore.Dto;

namespace DirectoryCore.Services.Auth0
{
    public class SupervisorAuthenticationInfo
    {
        public string AccessToken { get; set; }
        public int ExpiresIn { get; set; }
        public SupervisorDto Supervisor { get; set; }
    }
}
