namespace Willow.TwinLifecycleManagement.Web.Models
{
	/// <summary>
	/// Information about the logged in user
	/// </summary>
	public class AuthenticatedUserDto
	{
		/// <summary>
		/// Display name for this user
		/// </summary>
		public string DisplayName { get; set; }

		/// <summary>
		/// The groups that this user belongs to
		/// </summary>
		public string[] Groups { get; set; }

		/// <summary>
		/// User id
		/// </summary>
		public string Id { get; set; }

		/// <summary>
		/// Email
		/// </summary>
		public string Email { get; set; }

	}
}
