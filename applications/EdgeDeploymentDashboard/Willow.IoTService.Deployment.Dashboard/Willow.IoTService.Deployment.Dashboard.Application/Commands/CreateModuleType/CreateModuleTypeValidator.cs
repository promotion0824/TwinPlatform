namespace Willow.IoTService.Deployment.Dashboard.Application.Commands.CreateModuleType;

using System.Text.RegularExpressions;
using FluentValidation;

public class CreateModuleTypeValidator : AbstractValidator<CreateModuleTypeCommand>
{
    // version regex only in major.minor.patch format
    private static readonly Regex VersionRegex = new(@"^\d+\.\d+\.\d+$");

    // module type regex only contains characters, numbers, underscore
    // leading character must not be a number
    private static readonly Regex ModuleTypeRegex = new(@"^[a-zA-Z_][a-zA-Z0-9_]*$");

    public CreateModuleTypeValidator()
    {
        this.RuleFor(x => x.Version)
            .Must(x => VersionRegex.IsMatch(x))
            .WithMessage("Version must be in major.minor.patch format");

        this.RuleFor(x => x.ModuleType)
            .Must(x => ModuleTypeRegex.IsMatch(x))
            .WithMessage(
                "Module type must contain only characters, numbers, underscore and leading character must not be a number");
    }
}
