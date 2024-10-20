using System;
using Microsoft.AspNetCore.Authorization;
using MobileXL.Models;

namespace MobileXL.Security
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple=true, Inherited=true)]
    public class MobileAuthorizeAttribute : AuthorizeAttribute
    {
        public MobileAuthorizeAttribute()
        {
            Roles = string.Join(',', UserTypeNames.CustomerUser);
        }
    }
}
