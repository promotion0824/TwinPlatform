namespace AdminPortalXL.Models.Directory
{
    public class AuthenticationInfo
    {
        public string AccessToken { get; set; }
        public int ExpiresIn { get; set; }
        public Supervisor Supervisor { get; set; }
    }
}
