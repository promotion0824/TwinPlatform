namespace Willow.Api.Common.Extensions;

using Microsoft.Extensions.Configuration;

/// <summary>
/// A class containing extension methods for <see cref="IConfiguration"/>.
/// </summary>
public static class ConfigurationExtensions
{
    /// <summary>
    /// Parses a configuration section into an object.
    /// </summary>
    /// <typeparam name="T">The type to convert the configuration into.</typeparam>
    /// <param name="configuration">The IConfiguration object.</param>
    /// <param name="configurationKey">The name of the configuration entry to convert to the target type.</param>
    /// <returns>An object of Type T.</returns>
    public static T? ParseConfiguration<T>(this IConfiguration configuration, string configurationKey)
        => configuration.GetSection(configurationKey).Get<T>();

    /// <summary>
    /// Parses a configuration section into an object.
    /// </summary>
    /// <param name="configuration">The IConfiguration instance.</param>
    /// <param name="key">The key index for the section.</param>
    /// <param name="setAction">An action to execute on the section.</param>
    /// <returns>An IConfiguration Section.</returns>
    public static IConfigurationSection GetSection(
        this IConfiguration configuration,
        string key,
        Action<IConfigurationSection> setAction)
    {
        var section = configuration.GetSection(key);
        if (!section.GetChildren().Any())
        {
            setAction(section);
        }

        return section;
    }
}
