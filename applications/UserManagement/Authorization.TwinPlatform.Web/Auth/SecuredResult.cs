namespace Authorization.TwinPlatform.Web.Auth;
public record SecuredResult<T>
{
    public T Result { get; init; }

    public bool FailedAuthorization { get; init; }

    public SecuredResult(T result)
    {
        Result = result;
        FailedAuthorization = false;
    }

    public SecuredResult(T result, bool failedAuthorization)
    {
        Result = result;
        FailedAuthorization = failedAuthorization;    
    }
}
