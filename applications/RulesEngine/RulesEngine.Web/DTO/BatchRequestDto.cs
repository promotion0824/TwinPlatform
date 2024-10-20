using System;
using Willow.Rules.DTO;
using Willow.Rules.Model;

namespace RulesEngine.Web.DTO;

/// <summary>
/// A batch request
/// </summary>
public class BatchRequestDto
{
	/// <summary>
	/// Specifications on how to sort the batch
	/// </summary>
	public SortSpecificationDto[] SortSpecifications { get; init; } = Array.Empty<SortSpecificationDto>();

	/// <summary>
	/// Specification on how to filter the batch
	/// </summary>
	public FilterSpecificationDto[] FilterSpecifications { get; init; } = Array.Empty<FilterSpecificationDto>();

	/// <summary>
	/// The page number to return for the batch (one-based)
	/// </summary>
	public int? Page { get; init; }

	/// <summary>
	/// The amount of items in the batch
	/// </summary>
	public int? PageSize { get; init; }
}
