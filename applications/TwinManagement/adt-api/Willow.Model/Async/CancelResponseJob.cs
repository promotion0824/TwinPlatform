using System.Net;

namespace Willow.Model.Async;

public class CancelResponseJob
{
    public CancelResponseJob(HttpStatusCode statusCode)
    {
        StatusCode = statusCode;
    }
    public HttpStatusCode StatusCode { get; set; }
}
