using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Willow.Api.Authorization
{
    public interface IAuthorizationService
    {
        Task<bool> AssertPolicy(string policyName, ClaimsPrincipal user, IDictionary<string, object> parms, IDictionary<String, StringValues> headers);
    }
}
