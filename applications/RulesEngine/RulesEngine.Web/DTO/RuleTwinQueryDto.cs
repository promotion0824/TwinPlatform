// POCO class
#nullable disable

using Willow.Rules.Model;

namespace RulesEngine.Web
{
	/// <summary>
	/// Dto form of a <see cref="RuleTwinQuery"/>
	/// </summary>
	public class RuleTwinQueryDto
	{
		/// <summary>
		/// Model Ids involved in Query, primary model Id at index []zero
		/// </summary>
		/// <remarks>
		/// First version supports just one model id
		/// </remarks>
		public string[] ModelIds { get; }

		/// <summary>
		/// Creates a new <see cref="RuleTwinQueryDto" />
		/// </summary>
		/// <param name="primaryModelId"></param>
		public RuleTwinQueryDto(string primaryModelId)
		{
			this.ModelIds = new[] { primaryModelId };
		}

	}
}
