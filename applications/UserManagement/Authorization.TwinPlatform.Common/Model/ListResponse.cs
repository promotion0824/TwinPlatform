namespace Authorization.TwinPlatform.Common.Model;

/// <summary>
/// List Response Wrapper
/// </summary>
/// <typeparam name="T">Type of Entity Model</typeparam>
/// <param name="data">Collection of records.</param>
public class ListResponse<T>(IEnumerable<T> data)
{
    /// <summary>
    /// list of records
    /// </summary>
    public IEnumerable<T> Data { get; set; } = data;
}

