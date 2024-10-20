namespace Willow.Rules.Web.DTO
{
	/// <summary>
	/// Result of searching
	/// </summary>
	public class SearchResultDto
	{
		/// <summary>
		/// The incoming query
		/// </summary>
		public string Query { get; set; }

		/// <summary>
		/// The results
		/// </summary>
		public SearchLineDto[] Results { get; set; }
	}
}