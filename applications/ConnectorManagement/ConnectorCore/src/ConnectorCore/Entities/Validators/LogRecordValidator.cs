namespace ConnectorCore.Entities.Validators
{
    using FluentValidation;

    internal class LogRecordValidator : AbstractValidator<LogRecordEntity>
    {
        public LogRecordValidator()
        {
            RuleFor(x => x.Errors).NotNull().NotEmpty();
        }
    }
}
