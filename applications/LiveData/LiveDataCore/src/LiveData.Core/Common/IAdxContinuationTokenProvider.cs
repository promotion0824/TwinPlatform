namespace Willow.LiveData.Core.Common;

/// <summary>
/// Represents a provider for generating and parsing continuation tokens used for ADX paging.
/// </summary>
/// <typeparam name="T1">The type of the first item in the continuation token.</typeparam>
/// <typeparam name="T2">The type of the second item in the continuation token.</typeparam>
internal interface IAdxContinuationTokenProvider<T1, T2>
{
    T1 GetToken(T1 item1, T2 item2);

    (T1 Item1, T2 Item2) ParseToken(T1 token);
}
