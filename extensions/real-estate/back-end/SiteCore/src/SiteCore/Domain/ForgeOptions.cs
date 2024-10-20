namespace SiteCore.Domain
{
    public class ForgeOptions
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string[] Scope { get; set; } = new string[0];
        public string TokenEndpoint { get; set; }
    }
}
