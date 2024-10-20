namespace DirectoryCore.Configs
{
    public class Auth0Option
    {
        public string Domain { get; set; }

        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public string Audience { get; set; }

        public string ManagementClientId { get; set; }

        public string ManagementClientSecret { get; set; }

        public string ManagementAudience { get; set; }
    }
}
