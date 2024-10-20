using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

using Willow.DataValidation;

using Willow.Workflow;

namespace Willow.PlatformPortal.XL.UnitTests
{
    public class CreateCheckRequestTests
    {
        private List<(string Name, string Message)> _errors = new List<(string Name, string Message)>();
        private CreateCheckRequest _request;

        public CreateCheckRequestTests()
        {
            // Valid request
             _request = new CreateCheckRequest
            {
                Name            = "bob",
                Type            = CheckType.Numeric,
                TypeValue       = "3",
                DecimalPlaces   = 2
            };

        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData("\r")]
        public void CreateCheckRequest_Name_invalid(string name)
        {
            _request.Name = name;

           ShouldBeInvalid("Name", "Name is required");
        }

        [Theory]
        [InlineData(CheckType.List)]
        [InlineData(CheckType.Numeric)]
        [InlineData(CheckType.Date)]
        [InlineData(CheckType.Total)]
        public void CreateCheckRequest_Validate_success(CheckType type)
        {
            _request.Type = type;

            var result = _request.Validate(_errors);

            Assert.True(result);
            Assert.Empty(_errors);
        }

        [Theory]
        [InlineData(CheckType.Numeric, null)]
        [InlineData(CheckType.Total, null)]
        public void CreateCheckRequest_Validate_decimalPlaces_required(CheckType type, int? decimalPlaces)
        {
            _request.Type = type;
            _request.DecimalPlaces = decimalPlaces;

           ShouldBeInvalid("DecimalPlaces", "DecimalPlaces is required");
        }

        [Theory]
        [InlineData(CheckType.Numeric, -1)]
        [InlineData(CheckType.Numeric, 32)]
        [InlineData(CheckType.Total, -1)]
        [InlineData(CheckType.Total, 32)]
        public void CreateCheckRequest_Validate_invalid_decimalPlaces(CheckType type, int? decimalPlaces)
        {
            _request.Type = type;
            _request.DecimalPlaces = decimalPlaces;

           ShouldBeInvalid("DecimalPlaces", "DecimalPlaces must be between 1 and 4");
        }

        private void ShouldBeInvalid(string name, string message)
        {
            var result = _request.Validate(_errors);

            Assert.False(result);
            Assert.Single(_errors);
            Assert.Equal(name, _errors[0].Name);
            Assert.Equal(message, _errors[0].Message);
        }
    }
}
