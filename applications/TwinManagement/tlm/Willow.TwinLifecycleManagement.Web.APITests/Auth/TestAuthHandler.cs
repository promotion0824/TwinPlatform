using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Willow.TwinLifecycleManagement.Web.APITests
{
	public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
	{
		public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
			ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
			: base(options, logger, encoder, clock)
		{
		}

		protected override Task<AuthenticateResult> HandleAuthenticateAsync()
		{
			var claims = new[] {
				new Claim(ClaimTypes.Name, "Test user"),
				new Claim("emails", "test.user@fake.com"),
				new Claim(ClaimTypes.NameIdentifier,"fakeIdGuid")
			};
			var identity = new ClaimsIdentity(claims, "FakeBearer");
			var principal = new ClaimsPrincipal(identity);
			var ticket = new AuthenticationTicket(principal, "FakeBearer");

			var result = AuthenticateResult.Success(ticket);

			return Task.FromResult(result);
		}
	}
}
