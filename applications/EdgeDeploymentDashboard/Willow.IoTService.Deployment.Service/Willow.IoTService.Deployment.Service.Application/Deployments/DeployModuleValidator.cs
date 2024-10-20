namespace Willow.IoTService.Deployment.Service.Application.Deployments;

using FluentValidation;
using Willow.IoTService.Deployment.Common.Messages;

/// <inheritdoc />
public class DeployModuleValidator : AbstractValidator<IDeployModule>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DeployModuleValidator" /> class.
    /// </summary>
    public DeployModuleValidator()
    {
        this.RuleFor(x => x.DeploymentId)
            .NotEmpty();
        this.RuleFor(x => x.ModuleId)
            .NotEmpty();
    }
}
