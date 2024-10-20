using System.Collections.Concurrent;
using System.Net;

namespace Willow.Model.Responses;

public class MultipleEntityResponse
{
    public ConcurrentBag<EntityResponse> Responses { get; set; } = new ConcurrentBag<EntityResponse>();

    public MultipleEntityResponse Merge(MultipleEntityResponse other)
    {
        foreach (var r in other.Responses)
            Responses.Add(r);
        return this;
    }

    public HttpStatusCode StatusCode =>
        Responses.Any(r => r.StatusCode != HttpStatusCode.OK) ? HttpStatusCode.MultiStatus : HttpStatusCode.OK;
}

public class EntityResponse
{
    public HttpStatusCode StatusCode { get; set; }
    public string? EntityId { get; set; }
    public string? Operation { get; set; }
    public string? Message { get; set; }
    public string? SubEntityId { get; set; }
}
