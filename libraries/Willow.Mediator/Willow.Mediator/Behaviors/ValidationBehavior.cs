namespace Willow.Mediator.Behaviors;

using FluentValidation;
using MediatR;

/// <summary>
/// A MediatR pipeline behavior that validates requests.
/// </summary>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> validators;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationBehavior{TRequest, TResponse}"/> class.
    /// </summary>
    /// <param name="validators">A collection of validators.</param>
    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        this.validators = validators;
    }

    /// <summary>
    /// Handle the request.
    /// </summary>
    /// <param name="request">The incoming request.</param>
    /// <param name="next">The next call in the stack.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>An asynchronous task.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the count of invalid results is greater than 0.</exception>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (validators.Any())
        {
            var validators = await Task.WhenAll((IEnumerable<Task<FluentValidation.Results.ValidationResult>>)this.validators.Select(validator => validator.ValidateAsync(request, cancellationToken)));
            var result = validators
                .SelectMany(validationResult => validationResult.Errors)
                .Where(validationFailure => validationFailure != null)
                .ToList();

            if (result.Count > 0)
            {
                var error = string.Join(Environment.NewLine, result);
                throw new InvalidOperationException(error);
            }
        }

        return await next();
    }
}
