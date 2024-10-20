namespace Willow.CommandAndControl.Application.Filters;

internal class ValidationFilter<T>(IValidator<T> validator)
    : IEndpointFilter
    where T : class
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var validatable = context.Arguments.SingleOrDefault(x => x?.GetType() == typeof(T));
        if (validatable != null)
        {
            var result = await validator.ValidateAsync((T)validatable);

            if (result.IsValid)
            {
                return await next(context);
            }

            var details = new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "One or more validation errors occurred.",
            };
            details.Extensions.Add("traceId", context.HttpContext.TraceIdentifier);
            details.Extensions.Add("errors", result.Errors.Select(x => new
            {
                Field = x.PropertyName,
                Message = x.ErrorMessage,
            }).ToList());
            return Results.BadRequest(details);
        }

        return await next(context);
    }
}
