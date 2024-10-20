namespace Willow.LiveData.Core.Features.GetLiveData.GetPointLiveData.Adx;

using System;
using System.Security.Cryptography;
using System.Text;
using Willow.LiveData.Core.Common;

/// <summary>
/// Token Provider for StoredQueryResult for ADX for supporting paging.
/// </summary>
internal class AdxStoredQueryResultTokenProvider : IContinuationTokenProvider<string, string>
{
    /// <inheritdoc/>
    public string GetToken(string query)
    {
        return "SQR" + BitConverter.ToString(
                        MD5.Create().ComputeHash(
                            Encoding.ASCII.GetBytes(query))).Replace("-", string.Empty);
    }

    /// <inheritdoc/>
    public string ParseToken(string token)
    {
        throw new NotSupportedException("This cannot be parsed");
    }
}
