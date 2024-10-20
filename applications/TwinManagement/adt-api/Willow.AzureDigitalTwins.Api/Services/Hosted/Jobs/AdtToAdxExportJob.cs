using System.Threading;
using System.Threading.Tasks;
using Willow.AzureDigitalTwins.Api.Extensions;
using Willow.AzureDigitalTwins.Api.Persistence.Models.TwinsApi;
using Willow.Model.Jobs;

namespace Willow.AzureDigitalTwins.Api.Services.Hosted.Jobs;

/// <summary>
/// ADT to ADX Export Job Processor
/// </summary>
public class AdtToAdxExportJob(IExportService exportService) : BaseJob<AdtToAdxExportJobOption>, IJobProcessor
{
    public override string JobType => "TwinsApi";

    public override string JobSubType => "AdtToAdxExport";

    public Task ExecuteJobAsync(JobsEntry jobsEntry, JobExecutionContext jobExecutionContext, CancellationToken cancellationToken)
    {        
        var jobOption = jobsEntry.GetCustomData<AdtToAdxExportJobOption>();
        return exportService.Export(jobsEntry, jobOption, cancellationToken);
    }
}
