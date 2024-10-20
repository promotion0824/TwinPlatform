using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Willow.Rules.Repository;

// POCO class serialized to DB
#nullable disable

namespace Willow.Rules.Model;

/// <summary>
/// Review statuses used on a rule instance
/// </summary>
public enum ReviewStatus
{
	/// <summary>No review has occurred</summary>
	NotReviewed = 0,
	/// <summary>Review in progress</summary>
	InProgress = 1,
	/// <summary>Review complete</summary>
	Complete = 2,
}

/// <summary>
/// Records data about use of rule instances
/// </summary>
public class RuleInstanceMetadata : IId
{
	/// <summary>
	/// Same Id as the Rule Instance
	/// </summary>
	[JsonProperty("id")]
	public string Id { get; init; }

	/// <summary>
	/// Count number of times triggered
	/// </summary>
	public int TriggerCount { get; set; }

	/// <summary>
	/// Current version of the rule instance
	/// </summary>
	public int Version { get; set; }

	/// <summary>
	/// When this was last triggered
	/// </summary>
	public DateTimeOffset LastTriggered { get; set; }

	/// <summary>
	/// Review status used of the rule instance
	/// </summary>
	public ReviewStatus ReviewStatus { get; set; }

	/// <summary>
	/// When the last comment was posted
	/// </summary>
	public DateTimeOffset LastCommentPosted { get; set; }

	/// <summary>
	/// Total comments posted
	/// </summary>
	public int TotalComments { get; set; }

	/// <summary>
	/// Comments for this rule instance
	/// </summary>
	public IList<RuleComment> Comments { get; set; } = new List<RuleComment>(0);

	/// <summary>
	/// Tags
	/// </summary>
	public IList<string> Tags { get; set; } = new List<string>(0);

	/// <summary>
	/// Adds a comment
	/// </summary>
	public void AddComment(RuleComment comment)
	{
		if(Comments is null)
		{
			Comments = new List<RuleComment>();
		}

		Comments.Add(comment);

		TotalComments = Comments.Count;

		LastCommentPosted = comment.Created;
	}
}
