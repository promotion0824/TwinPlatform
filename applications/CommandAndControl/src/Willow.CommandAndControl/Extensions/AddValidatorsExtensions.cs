namespace Willow.CommandAndControl.Extensions;

internal static class AddValidatorsExtensions
{
    public static IServiceCollection AddValidators(this IServiceCollection services)
    {
        services.AddScoped<IValidator<PostRequestedCommandsDto>, PostRequestedCommandsValidator>();
        services.AddScoped<IValidator<GetRequestedCommandByIdDto>, GetRequestedCommandByIdValidator>();
        services.AddScoped<IValidator<GetResolvedCommandByIdRequestDto>, GetResolvedCommandByIdValidator>();
        services.AddScoped<IValidator<UpdateRequestedCommandStatusDto>, UpdateRequestedCommandStatusValidator>();
        services.AddScoped<IValidator<UpdateRequestedCommandsStatusDto>, UpdateRequestedCommandsStatusValidator>();
        services.AddScoped<IValidator<UpdateResolvedCommandStatusDto>, UpdateResolvedCommandStatusValidator>();
        return services;
    }
}
