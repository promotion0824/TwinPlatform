namespace Willow.FeatureFlagsProvider.FeatureFlags;

using ConfigCat.Client;

/// <summary>
/// The feature flags provider.
/// </summary>
public interface IFeatureFlagsProvider
{
    /// <summary>
    /// Get a feature flag.
    /// </summary>
    /// <param name="featureFlag">The name of the feature flag.</param>
    /// <param name="defaultValue">The default value of the feature flag to return if the flag is not found.</param>
    /// <param name="featureFlagUser">What user to use to get the feature flag for.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<bool> GetFeatureFlag(
        string featureFlag,
        bool defaultValue,
        FeatureFlagUser? featureFlagUser = null);

    /// <summary>
    /// Get a text setting.
    /// </summary>
    /// <param name="textSetting">The text setting name.</param>
    /// <param name="defaultValue">The default value of the textSetting to return if the flag is not found.</param>
    /// <param name="featureFlagUser">What user to use to get the text setting for.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<string> GetTextSetting(
        string textSetting,
        string defaultValue,
        FeatureFlagUser? featureFlagUser = null);

    /// <summary>
    /// Get a number setting.
    /// </summary>
    /// <param name="numberSetting">The name of the number setting to return.</param>
    /// <param name="defaultValue">The default value of the number setting to return if the flag is not found.</param>
    /// <param name="featureFlagUser">What user to use to get the number setting for.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<int> GetNumberSetting(
        string numberSetting,
        int defaultValue,
        FeatureFlagUser? featureFlagUser = null);
}

/// <summary>
/// The config cat feature flags provider.
/// </summary>
public class ConfigCatFeatureFlagsProvider : IFeatureFlagsProvider
{
    private readonly IConfigCatClient client;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigCatFeatureFlagsProvider"/> class.
    /// </summary>
    /// <param name="client">An instance of the config cat client.</param>
    public ConfigCatFeatureFlagsProvider(IConfigCatClient client)
    {
        this.client = client;
    }

    /// <inheritdoc/>
    public Task<bool> GetFeatureFlag(
        string featureFlag,
        bool defaultValue,
        FeatureFlagUser? featureFlagUser = null)
    {
        return GetValueAsync(featureFlag, defaultValue, featureFlagUser);
    }

    /// <inheritdoc/>
    public Task<string> GetTextSetting(
        string textSetting,
        string defaultValue,
        FeatureFlagUser? featureFlagUser = null)
    {
        return GetValueAsync(textSetting, defaultValue, featureFlagUser);
    }

    /// <inheritdoc/>
    public Task<int> GetNumberSetting(
        string numberSetting,
        int defaultValue,
        FeatureFlagUser? featureFlagUser = null)
    {
        return GetValueAsync(numberSetting, defaultValue, featureFlagUser);
    }

    private Task<T> GetValueAsync<T>(
        string featureFlag,
        T defaultValue,
        FeatureFlagUser? featureFlagUser = null)
    {
        User? user = default;

        if (featureFlagUser != null)
        {
            user = new User(featureFlagUser.UserId)
            {
                Email = featureFlagUser.Email,
                Custom = featureFlagUser.CustomAttributes,
            };
        }

        return client.GetValueAsync(featureFlag, defaultValue, user);
    }
}
