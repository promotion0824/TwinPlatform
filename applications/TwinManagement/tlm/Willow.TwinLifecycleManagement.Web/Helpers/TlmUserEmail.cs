using System.Net.Mail;
using System.Security.Claims;
using System.Text;

namespace Willow.TwinLifecycleManagement.Web.Helpers
{
	public class TlmUserEmail : MailAddress
	{
		public TlmUserEmail(string address) : base(address)
		{
		}

		public TlmUserEmail(string address, string displayName) : base(address, displayName)
		{
		}

		public TlmUserEmail(string address, string displayName, Encoding displayNameEncoding) : base(address, displayName, displayNameEncoding)
		{
		}

		public TlmUserEmail(ClaimsPrincipal userClaims) : base(userClaims.FindFirst("emails")?.Value ?? userClaims.FindFirstValue(ClaimTypes.Email))
		{
		}
	}
}
