namespace ConnectorCore.Common.Abstractions
{
    internal interface IContinuationTokenProvider<T, TOut>
    {
        string GetToken(T item);

        TOut ParseToken(string token);
    }
}
