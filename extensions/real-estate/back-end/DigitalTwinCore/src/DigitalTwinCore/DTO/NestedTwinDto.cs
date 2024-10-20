using DigitalTwinCore.Models;
using System.Collections.Generic;
using System.Linq;

namespace DigitalTwinCore.Dto;

public class NestedTwinDto
{
    public TwinDto Twin { get; set; }
    public IList<NestedTwinDto> Children { get; set; }

    public static NestedTwinDto MapFrom(NestedTwin nestedTwin)
    {
        if (nestedTwin == null)
        {
            return null;
        }

        return new NestedTwinDto
        {
            Twin = TwinDto.MapFrom(nestedTwin.Twin),
            Children = nestedTwin.Children?.Select(MapFrom).ToList()
        };
    }

    public static List<NestedTwinDto> MapFrom(IEnumerable<NestedTwin> nestedTwinList)
    {
        if (nestedTwinList == null)
        {
            return null;
        }

        return nestedTwinList.Select(MapFrom).ToList();
    }
}
