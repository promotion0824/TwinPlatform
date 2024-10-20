namespace MobileXL.Controllers
{
    public class SignInRequest
    {
        public string AuthorizationCode { get; set; }
        public string RedirectUri { get; set; }
        public string CodeVerifier { get; set; }
        public SignInType SignInType { get; set; }
    }

    public enum SignInType
    {
        SignIn = 0,
        ResetPassword = 1,
        SilentRenew = 2
    }
}
