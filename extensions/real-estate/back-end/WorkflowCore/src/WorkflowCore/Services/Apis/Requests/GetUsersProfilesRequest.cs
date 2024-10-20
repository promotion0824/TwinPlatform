using System.Collections.Generic;
using System;
using System.Linq;

namespace WorkflowCore.Services.Apis.Requests;

public record GetUsersProfilesRequest
{
    /// <summary>
    /// make sure each list in the request is distinct
    /// </summary>
    /// <param name="ids"></param>
    /// <param name="emails"></param>
    public GetUsersProfilesRequest(List<Guid> ids, List<string> emails)
    {
        Ids = ids?.Distinct().ToList();
        Emails = emails?.Distinct().ToList();
    }
    /// <summary>
    /// user ids
    /// </summary>
    public List<Guid> Ids { get; private init; }
    /// <summary>
    /// user emails
    /// </summary>
    public List<string> Emails { get; private init; }
}

