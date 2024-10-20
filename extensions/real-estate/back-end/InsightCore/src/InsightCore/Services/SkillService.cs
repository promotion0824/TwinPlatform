using System.Linq;
using System.Threading.Tasks;
using InsightCore.Dto;
using Microsoft.Extensions.Logging;
using Willow.Batch;

namespace InsightCore.Services;

public interface ISkillService
{
    Task<BatchDto<SkillDto>> GetSkillsAsync(BatchRequestDto request,bool ignoreQueryFilters = false);
}

public class SkillService : ISkillService
{
    private readonly IInsightRepository _repository;
    private readonly ILogger<SkillService> _logger;

    public SkillService(IInsightRepository repository, ILogger<SkillService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<BatchDto<SkillDto>> GetSkillsAsync(BatchRequestDto request,bool ignoreQueryFilters = false)
    {
        var batchInsights = await _repository.GetSkillsAsync(request, ignoreQueryFilters);
        // temporary fix for the issue with the null ruleId values in the database
        batchInsights.Items.Where(x => x.Id == null).ToList().ForEach(c =>
        {
            c.Id = "inspection_note_";
            c.Name = "Inspection Note";

        });
        return new BatchDto<SkillDto>
        {
            After = batchInsights.After,
            Before = batchInsights.Before,
            Items = batchInsights.Items,
            Total = batchInsights.Total
        };
    }
}
