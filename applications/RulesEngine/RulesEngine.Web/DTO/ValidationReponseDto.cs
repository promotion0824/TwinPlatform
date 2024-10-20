// POCO class
#nullable disable

namespace RulesEngine.Web
{
	/// <summary>
	/// Validation results from a post to server
	/// </summary>
	public class ValidationReponseDto
	{
		/// <summary>
		/// Fields and validation messages
		/// </summary>
		public ValidationReponseElementDto[] results { get; set; }
	}

}
