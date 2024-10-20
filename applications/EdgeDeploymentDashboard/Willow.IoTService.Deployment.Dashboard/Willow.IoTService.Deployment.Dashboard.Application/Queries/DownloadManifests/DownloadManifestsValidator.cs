namespace Willow.IoTService.Deployment.Dashboard.Application.Queries.DownloadManifests;

using FluentValidation;

public class DownloadManifestsValidator : AbstractValidator<DownloadManifestsQuery>
{
    private const int DownloadManifestsMaxCount = 10;

    public DownloadManifestsValidator()
    {
        this.RuleFor(x => x.DeploymentIds)
           .NotEmpty()
           .Must(x => x.Count() < DownloadManifestsMaxCount);
    }
}
