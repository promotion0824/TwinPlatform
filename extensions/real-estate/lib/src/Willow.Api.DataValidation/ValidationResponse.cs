using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Willow.DataValidation;

namespace Willow.Api.DataValidation
{
    public static class ValidationResponse
    {
        public static ActionResult Handle(ActionContext ctx)
        { 
            ActionExecutingContext context = ctx as ActionExecutingContext;

            if(context != null && context.ActionArguments != null)
            { 
                // Go through all the controller args and if it's a class then do a validate
                foreach(var argValue in context.ActionArguments.Values)
                {
                    if(argValue == null)
                        continue;

                    if(argValue is string || !argValue.GetType().IsClass)
                        continue;

                    var errors = new List<(string Name, string Message)>();

                    if(!argValue.Validate(errors))
                    {
                        return new UnprocessableEntityObjectResult(new ValidationError { Items = errors.Select(i=> new ValidationErrorItem(i.Name, i.Message)).ToList() });
                    }
                }
            }
                        
            return new BadRequestObjectResult(context.ModelState);
        }
    }
}
