namespace Willow.TwinLifecycleManagement.Web.Auth
{
	/// <summary>
	/// Options for validating JWT tokens
	/// </summary>
	/// <remarks>
	/// When a JWT is received we will check the authority, issuer and audience to make sure they are as expected
	/// </remarks>
	public class JwtBearerConfig
	{
		/// <summary>
		/// Name of config section in appsettings
		/// </summary>
		public const string Config = "Jwt";

		/// <summary>
		/// Authority, e.g.
		/// </summary>
		public string Authority { get; set; }

		/// <summary>
		/// Issuer, e.g. "https://willowdevb2c.b2clogin.com/a80618f8-f5e9-43bf-a98f-107dd8f54aa9/v2.0/"
		/// </summary>
		public string Issuer { get; set; }

		/// <summary>
		/// Audience, e.g. "6bb6cec6-8309-4891-9b25-42a3ef3247ec"
		/// </summary>
		public string Audience { get; set; }
	}
}
