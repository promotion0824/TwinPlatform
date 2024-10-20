namespace Willow.LiveData.Core.Tests.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Moq;
    using NUnit.Framework;
    using Willow.LiveData.Core.Common;
    using Willow.LiveData.Core.Features.GetLiveData.GetPointLiveData;
    using Willow.LiveData.Core.Infrastructure.Attributes;

    public class DateRangeValidationAttributeTests
    {
        [TestCase]
        public void DateRangeValidationAttribute_Returns_Validates_the_Date_Range()
        {
            //arrange
            var dateRangeValidationAttribute = new DateRangeValidationAttribute();
            var modelState = new ModelStateDictionary();
            modelState.AddModelError("name", "invalid");
            var argument = new Dictionary<string, object>();
            argument.Add("start", DateTime.Now.AddDays(-10));
            argument.Add("end", DateTime.Now);
            var actionContext = new ActionContext(
                Mock.Of<HttpContext>(),
                Mock.Of<Microsoft.AspNetCore.Routing.RouteData>(),
                Mock.Of<ActionDescriptor>(),
                modelState);

            var actionExecutingContext = new ActionExecutingContext(
                actionContext,
                new List<IFilterMetadata>(),
                argument,
                Mock.Of<Controller>());

            //act
            dateRangeValidationAttribute.OnActionExecuting(actionExecutingContext);

            //assert
        }

        [TestCase]
        public void DateRangeValidationAttribute_Throws_InvalidCastException_When_Date_Is_Not_Valid()
        {
            //arrange
            var dateRangeValidationAttribute = new DateRangeValidationAttribute();
            var modelState = new ModelStateDictionary();
            modelState.AddModelError("name", "invalid");
            var argument = new Dictionary<string, object>();
            argument.Add("start", "2022-01-01");
            argument.Add("end", "2022-01-01");
            var actionContext = new ActionContext(
                Mock.Of<HttpContext>(),
                Mock.Of<Microsoft.AspNetCore.Routing.RouteData>(),
                Mock.Of<ActionDescriptor>(),
                modelState);

            var actionExecutingContext = new ActionExecutingContext(
                actionContext,
                new List<IFilterMetadata>(),
                argument,
                Mock.Of<Controller>());

            //act
            Action action = () => dateRangeValidationAttribute.OnActionExecuting(actionExecutingContext);

            //assert
            action.Should().Throw<InvalidCastException>();
        }
    }
}
