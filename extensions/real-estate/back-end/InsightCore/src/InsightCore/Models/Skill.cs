using InsightCore.Dto;
using System.Collections.Generic;
using System.Linq;

namespace InsightCore.Models;
public class Skill
{
    public string Id { get; set; }
    public string Name { get; set; }
    public InsightType Category { get; set; }

    public static SkillDto MapTo(Skill model)
    {
        return model==null?null: new SkillDto
        {
            Id = model.Id,
            Name = model.Name,
            Category = model.Category
        };
    }
    public static List<SkillDto> MapTo(IEnumerable<Skill> entities)
    {
        return entities?.Select(MapTo).ToList();
    }
}
