using Willow.Rules.Model;

namespace RulesEngine.Web
{
	/// <summary>
	/// A rule template id and name
	/// </summary>
	public class RuleTemplateDto
	{
		/// <summary>
		/// Id of rule template
		/// </summary>
		public string Id { get; set; }

		/// <summary>
		/// Name of rule template
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Description
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// Sample elements
		/// </summary>
		public RuleUIElementDto[] Elements { get; set; }

		/// <summary>
		/// Parameters
		/// </summary>
		public RuleUIElementDto[] Parameters { get; set; }

		/// <summary>
		/// Scores
		/// </summary>
		public RuleUIElementDto[] ImpactScores { get; set; }
	}
}
