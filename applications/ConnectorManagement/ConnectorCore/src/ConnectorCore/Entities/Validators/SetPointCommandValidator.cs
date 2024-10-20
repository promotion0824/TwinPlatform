namespace ConnectorCore.Entities.Validators
{
    using System;
    using FluentValidation;

    internal class SetPointCommandValidator : AbstractValidator<SetPointCommandEntity>
    {
        public SetPointCommandValidator()
        {
            RuleFor(x => x.Id).NotEqual(Guid.Empty);
            RuleFor(x => x.SiteId).NotEqual(Guid.Empty);
            RuleFor(x => x.ConnectorId).NotEqual(Guid.Empty);
            RuleFor(x => x.EquipmentId).NotEqual(Guid.Empty);
            RuleFor(x => x.InsightId).NotEqual(Guid.Empty);
            RuleFor(x => x.PointId).NotEqual(Guid.Empty);
            RuleFor(x => x.SetPointId).NotEqual(Guid.Empty);
            RuleFor(x => x.Unit).NotNull();
            RuleFor(x => x.Type).IsInEnum();
            RuleFor(x => x.DesiredDurationMinutes).GreaterThan(0);
            RuleFor(x => x.Status).IsInEnum();
        }
    }
}
