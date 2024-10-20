namespace Willow.IoTService.Deployment.DbSync.Application;

using ConnectorCore.Contracts;
using FluentValidation;

/// <summary>
///     Fluent validation for ConnectorMessage.
/// </summary>
public class ConnectorMessageValidator : AbstractValidator<IConnectorMessage>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ConnectorMessageValidator" /> class.
    /// </summary>
    /// <remarks>
    ///     Validates the following:
    ///     1. ConnectorId is not empty.
    ///     2. CustomerId is not empty.
    /// </remarks>
    public ConnectorMessageValidator()
    {
        RuleFor(x => x.ConnectorId).NotEmpty();
        RuleFor(x => x.CustomerId).NotEmpty();
    }
}
