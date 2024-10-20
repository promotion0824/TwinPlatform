using System.Collections.Generic;

namespace WorkflowCore.Dto;

/// <summary>
/// Represents the count of tickets in each category.
/// </summary>
public class TicketCategoryCountDto
{
    /// <summary>
    /// Count of tickets in each category
    /// </summary>
    public List<CategoryCountDto> CategoryCounts { get; set; } = new();

    /// <summary>
    /// Count of tickets in other categories
    /// </summary>
    public int OtherCount { get; set; }
}
public record CategoryCountDto(string CategoryName, int Count);

