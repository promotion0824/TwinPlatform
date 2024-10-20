using System.Threading.Tasks;
using Willow.Model.Adt;
using Willow.Model.Requests;
using Willow.Model.Mapping;

namespace Willow.AzureDigitalTwins.Api.Services;

public interface IHealthService
{
    public Task CheckAllHealth();
}

public class HealthService : IHealthService
{
    private readonly IAcsService _acsService;
    private readonly IAdxService _adxService;
    private readonly ITwinsService _twinService;
    private readonly IMappingService _mappingService;

    public HealthService(
        IAcsService acsService,
        IAdxService adxService,
        ITwinsService twinsService,
        IMappingService mappingService,
        IJobsService jobsService)
    {
        _acsService = acsService;
        _adxService = adxService;
        _twinService = twinsService;
        _mappingService = mappingService;
    }

    public async Task CheckAllHealth()
    {
        var adtHealth = CheckAdtHealth();
        var adxHealth = CheckAdxHealth();
        var searchHealth = CheckSearchHealth();
        var mappingDbHealth = CheckMappingDbHealth();
        await Task.WhenAll(adtHealth, adxHealth, searchHealth, mappingDbHealth);
    }

    private async Task CheckAdxHealth()
    {
        await _adxService.GetTwins(new GetTwinsInfoRequest() { SourceType = SourceType.Adx }, pageSize: 1);
    }

    private async Task CheckAdtHealth()
    {
        await _twinService.GetTwins(new GetTwinsInfoRequest() { SourceType = SourceType.AdtQuery }, pageSize: 1);
    }

    private async Task CheckSearchHealth()
    {
        await _acsService.QueryRawUnifiedIndexAsync(rawQuery: "Type:twin", new Azure.Search.Documents.SearchOptions() { Size = 1 });
    }

    private async Task CheckMappingDbHealth()
    {
        await _mappingService.GetMappedEntriesAsync(new MappedEntryRequest() { }) ;
    }
}
