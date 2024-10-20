using Microsoft.Extensions.Logging;
using PlatformPortalXL.ServicesApi.InsightApi;

namespace PlatformPortalXL.Services;

public interface ISkillService
{

}
public class SkillService : ISkillService
{
    private readonly IInsightApiService _insightApi;
    private readonly ILogger<SkillService> _logger;


    public SkillService(IInsightApiService insightApi,
        ILogger<SkillService> logger)
    {
        _insightApi = insightApi;
        _logger = logger;
    }


}
