// POCO class serialized to DB
#nullable disable

using System;

namespace Willow.Rules.Model;

/// <summary>
/// Comments for rule instance review
/// </summary>
public class RuleComment
{
	/// <summary>
	/// The comment made
	/// </summary>
	public string Comment { get; set; }

	/// <summary>
	/// When the comment was created
	/// </summary>
	public DateTimeOffset Created { get; set; }

	/// <summary>
	/// Which user posted the comment
	/// </summary>
	public string User { get; set; }
}
