using System;
using Microsoft.AspNetCore.Authorization;

namespace AdminPortalXL.Security
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple=true, Inherited=true)]
    public class AuthorizeForSupervisorAttribute : AuthorizeAttribute
    {
        public AuthorizeForSupervisorAttribute()
        {
            Roles = UserRoles.Supervisor;
        }
    }
}
