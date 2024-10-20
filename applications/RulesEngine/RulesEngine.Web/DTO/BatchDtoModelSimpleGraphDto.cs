using Willow.Rules.Model;
using WillowRules.DTO;

namespace RulesEngine.Web.DTO;

/// <summary>
/// Simplified graph to hand back to UI standardised for filtering and sorting of Nodes
/// </summary>
public class BatchDtoModelSimpleGraphDto : BatchDto<ModelSimpleDto>
{
	/// <summary>
	/// Creates a new Batch
	/// </summary>
	public BatchDtoModelSimpleGraphDto(Batch<ModelSimpleDto> batch, ModelSimpleRelationshipDto[] relationships)
		: base(batch.QueryString, batch.Before, batch.After, batch.Total, batch.Items, batch.Next)
	{
		this.Relationships = relationships;
	}

	/// <summary>
	/// Edges
	/// </summary>/
	public ModelSimpleRelationshipDto[] Relationships { get; set; }
}
