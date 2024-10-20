namespace Willow.LiveData.Core.Features.GetLiveData.GetPointLiveData;

using System;
using Willow.LiveData.Core.Common;

/// <summary>
/// Continuation Token provider class for ADX paging.
/// </summary>
internal class AdxCTokenProvider : IAdxContinuationTokenProvider<string, int>
{
    /// <inheritdoc />
    public string GetToken(string item1, int item2)
    {
        return $"{item1}-{Math.Abs(item2)}";
    }

    /// <inheritdoc />
    public (string, int) ParseToken(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return (string.Empty, 0);
        }

        if (!token.Contains("-"))
        {
            throw new InvalidCastException($"Invalid token. Please provide valid continuation token");
        }

        try
        {
            string[] str = token.Split('-');
            int.TryParse(str[1], out int rowNumber);
            return (str[0], rowNumber);
        }
        catch (Exception)
        {
            return (string.Empty, 0);
        }
    }
}
