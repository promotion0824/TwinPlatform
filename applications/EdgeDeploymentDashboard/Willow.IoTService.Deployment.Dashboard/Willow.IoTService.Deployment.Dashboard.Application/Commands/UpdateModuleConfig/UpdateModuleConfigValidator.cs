namespace Willow.IoTService.Deployment.Dashboard.Application.Commands.UpdateModuleConfig;

using System.Text.Json;
using FluentValidation;

public class UpdateModuleConfigValidator : AbstractValidator<UpdateModuleConfigCommand>
{
    public UpdateModuleConfigValidator()
    {
        this.RuleFor(x => x.Id)
            .NotEmpty();
        this.RuleFor(x => x.Environment)
            .Must(
                x =>
                {
                    if (x == null)
                    {
                        return false;
                    }

                    try
                    {
                        JsonDocument.Parse(x);
                        return true;
                    }
                    catch (JsonException)
                    {
                        return false;
                    }
                })
            .WithMessage("Environment is not a valid JSON")
            .When(x => x.Environment != null);
        this.RuleFor(x => x.DeviceName)
            .NotEmpty()
            .When(x => x.DeviceName != null);
        this.RuleFor(x => x.IoTHubName)
            .NotEmpty()
            .When(x => x.IoTHubName != null);
    }
}
