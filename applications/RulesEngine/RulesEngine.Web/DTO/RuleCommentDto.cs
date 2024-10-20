// POCO class serialized to DB
#nullable disable

using System;
using Willow.Rules.Model;

namespace RulesEngine.Web;

/// <summary>
/// Comments for rule instance review
/// </summary>
public class RuleCommentDto
{
    /// <summary>
    /// Constructor for json
    /// </summary>
    public RuleCommentDto()
    {

    }

    /// <summary>
    /// Constructor for json
    /// </summary>
    public RuleCommentDto(RuleComment comment)
    {
        this.Comment = comment.Comment;
        this.Created = comment.Created;
        this.User = comment.User;
    }

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
