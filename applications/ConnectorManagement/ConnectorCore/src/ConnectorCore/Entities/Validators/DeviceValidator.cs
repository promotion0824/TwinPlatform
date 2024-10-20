namespace ConnectorCore.Entities.Validators
{
    using System;
    using FluentValidation;

    internal class DeviceValidator : AbstractValidator<DeviceEntity>
    {
        public DeviceValidator()
        {
            RuleFor(x => x.Name).MaximumLength(64);
            RuleFor(x => x.ClientId).NotNull().NotEqual(Guid.Empty);
            RuleFor(x => x.ExternalDeviceId).NotNull().NotEmpty();
            RuleFor(x => x.RegistrationId).MaximumLength(64);
            RuleFor(x => x.RegistrationKey).MaximumLength(256);
            RuleFor(x => x.Metadata).NotNull().NotEmpty();
            RuleFor(x => x.ConnectorId).NotNull().NotEqual(Guid.Empty);
        }
    }
}
