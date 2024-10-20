using System;
using System.Collections.Generic;
using System.Linq;

namespace DirectoryCore.Dto.Requests;

public class GetUsersProfilesRequest
{
    public GetUsersProfilesRequest()
    {
        Ids = Enumerable.Empty<Guid>().ToList();
        Emails = Enumerable.Empty<string>().ToList();
    }

    /// <summary>
    /// List of user ids
    /// </summary>
    public List<Guid> Ids { get; set; }

    /// <summary>
    /// List of user emails
    /// </summary>
    public List<string> Emails { get; set; }
}
