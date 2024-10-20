namespace AdminPortalXL.Controllers
{
    public class SignInRequest
    {
        public string AuthorizationCode { get; set; }
        public string RedirectUri { get; set; }
    }
}
