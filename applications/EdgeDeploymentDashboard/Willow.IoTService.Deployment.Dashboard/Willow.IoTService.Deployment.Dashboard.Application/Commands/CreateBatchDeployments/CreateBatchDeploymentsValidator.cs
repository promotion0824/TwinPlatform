namespace Willow.IoTService.Deployment.Dashboard.Application.Commands.CreateBatchDeployments;

using FluentValidation;

public class CreateBatchDeploymentsValidator : AbstractValidator<CreateBatchDeploymentsCommand>
{
    public CreateBatchDeploymentsValidator()
    {
        this.RuleFor(x => x.CreateDeploymentCommands)
           .Must(
               x =>
            {
                var commands = x.ToList();

                // distinct by model id should be equal to the number of commands
                return commands.Select(y => y.ModuleId)
                               .Distinct()
                               .Count() ==
                       commands.Count;
            })
           .WithMessage("Module Ids must be unique");

        // each command should have module id not empty
        this.RuleForEach(x => x.CreateDeploymentCommands)
           .Must(x => x.ModuleId != Guid.Empty)
           .WithMessage("Module Id must not be empty");

        // each command should have version not empty
        this.RuleForEach(x => x.CreateDeploymentCommands)
           .Must(x => !string.IsNullOrWhiteSpace(x.Version))
           .WithMessage("Version must not be empty");
    }
}
