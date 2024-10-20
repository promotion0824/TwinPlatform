namespace PlatformPortalXL.Services.ArcGis
{
    public class TokenResponse
    {
        public string Token { get; set; }
        public long Expires { get; set; }
        public bool Ssl { get; set; }
    }
}
