// POCO class
#nullable disable

namespace RulesEngine.Web
{
	/// <summary>
	/// Validation results from a post to server
	/// </summary>
	public class ValidationReponseElementDto
	{
		/// <summary>
		/// Creates a new ValidationResponseElementDto
		/// </summary>
		public ValidationReponseElementDto(string field, string message, string parentField = null)
		{
			this.field = field;
			this.message = message;
            this.parentField = parentField;
		}

		/// <summary>
		/// Field name
		/// </summary>
		public string field { get; set; }

		/// <summary>
		/// Message
		/// </summary>
		public string message { get; set; }

        /// <summary>
        /// Parent field name which is useful for validation highlights
        /// </summary>
        public string parentField { get; set; }
    }

}
