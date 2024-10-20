using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;

namespace Willow.Api.Authorization
{
    public class ApiAuthorizationFilter : IActionFilter
    {
        private readonly IAuthorizationService _authService;

        public ApiAuthorizationFilter(IAuthorizationService authService)
        {
            _authService = authService;
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // Do nothing	
        }

        public async void OnActionExecuting(ActionExecutingContext context)
        {
            var user = context.HttpContext.User;
            var parms = context.ActionArguments;
            var headers = context.HttpContext.Request.Headers;
            var policyName = PolicyFromDescriptor(context.ActionDescriptor.DisplayName) ?? "";

            if (!await _authService.AssertPolicy(policyName, user, parms, headers))
                context.Result = new UnauthorizedResult();
        }

        #region Private

        // Ex: Willow.Directory.Api.Controllers.CustomerController.Get (Willow.Directory.Api)
        private string PolicyFromDescriptor(string descriptorName)
        {
            var assemblyName = descriptorName.Substring(descriptorName.IndexOf("(") + 1).Trim().Replace(")", "");
            var typeName = descriptorName.Substring(0, descriptorName.IndexOf("(")).Trim();
            var methodName = typeName.Substring(typeName.LastIndexOf(".") + 1).Trim();
            typeName = typeName.Substring(0, typeName.LastIndexOf(".")).Trim();
            var assembly = Assembly.GetEntryAssembly();
            var type = assembly.GetTypes().Where(t => typeName == t.FullName).FirstOrDefault();
            var method = type?.GetMethod(methodName);
            var policyAttribute = method?.GetCustomAttribute<PolicyAttribute>();

            return policyAttribute?.Name;
        }

        #endregion
    }
}
