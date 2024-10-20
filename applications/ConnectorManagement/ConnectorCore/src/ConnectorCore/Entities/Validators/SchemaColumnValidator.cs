namespace ConnectorCore.Entities.Validators
{
    using FluentValidation;

    internal class SchemaColumnValidator : AbstractValidator<SchemaColumnEntity>
    {
        public SchemaColumnValidator()
        {
            RuleFor(x => x.Name).NotNull().NotEmpty().MaximumLength(64);
            RuleFor(x => x.IsRequired).NotNull();
            RuleFor(x => x.DataType).NotNull().NotEmpty().MaximumLength(64);
        }
    }
}
