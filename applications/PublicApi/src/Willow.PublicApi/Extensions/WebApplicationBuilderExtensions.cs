namespace Willow.PublicApi.Extensions;

using Microsoft.Extensions.Options;

internal static class WebApplicationBuilderExtensions
{
    public static T GetOptions<T>(this WebApplicationBuilder builder)
        where T : class =>
            builder.Services.BuildServiceProvider().GetRequiredService<IOptions<T>>().Value;

    public static T GetOptions<T>(this WebApplication app)
        where T : class =>
            app.Services.GetRequiredService<IOptions<T>>().Value;
}
