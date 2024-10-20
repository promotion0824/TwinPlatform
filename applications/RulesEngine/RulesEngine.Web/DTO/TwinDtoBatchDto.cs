using System.Collections.Generic;
using Willow.Rules.Model;
using WillowRules.DTO;

namespace RulesEngine.Web.DTO;

/// <summary>
/// TwinDto Batch Dto
/// </summary>
public class TwinDtoBatchDto : BatchDto<TwinDto>
{
    /// <summary>
    /// Creates a new BatchDto from a <see cref="Batch{T}"/>
    /// </summary>
    public TwinDtoBatchDto(Batch<TwinDto> batch, List<TwinDtoContentType> contentTypes)
        : base(batch)
    {
        ContentTypes = contentTypes;
    }

    /// <summary>
    /// A unique list of content types for the twins' extended data
    /// </summary>
    public List<TwinDtoContentType> ContentTypes { get; init; }
}
