using InsightCore.Models;

namespace InsightCore.Dto;
public class SkillDto
{
    public string Id { get; set; }
    public string Name { get; set; }
    public InsightType Category { get; set; }
}
