namespace ConnectorCore.Entities.Validators
{
    using System;
    using FluentValidation;

    internal class ConnectorValidator : AbstractValidator<ConnectorEntity>
    {
        public ConnectorValidator()
        {
            RuleFor(x => x.Name).NotNull().NotEmpty().MaximumLength(64);
            RuleFor(x => x.ClientId).NotNull().NotEqual(Guid.Empty);
            RuleFor(x => x.ConnectorTypeId).NotNull().NotEqual(Guid.Empty);
        }
    }
}
