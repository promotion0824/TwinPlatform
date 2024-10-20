using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

using Willow.DataValidation;

using Willow.Workflow;

namespace Willow.PlatformPortal.XL.UnitTests
{
    public class CreateInspectionRequestTests
    {
        private List<(string Name, string Message)> _errors = new List<(string Name, string Message)>();
        private CreateInspectionRequest _request;

        public CreateInspectionRequestTests()
        {
            // Valid request
             _request = new CreateInspectionRequest
            {
                Name                  = "bob",
                Frequency             = 4,
                FrequencyUnit                  = SchedulingUnit.Hours,
                StartDate             = "2021-01-02T01:01:01",
                EndDate               = "2031-01-02T01:01:01",

                ZoneId                = Guid.NewGuid(),
                AssetId               = Guid.NewGuid(),
                AssignedWorkgroupId   = Guid.NewGuid(),

                FloorCode             = "F21",

                Checks = new List<CreateCheckRequest> { new CreateCheckRequest 
                                                        { 
                                                            Name = "fred",
                                                            Type = CheckType.Numeric,
                                                            DecimalPlaces = 0,
                                                            TypeValue = "4"
                                                        }
                                                      }
            };

        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(4)]
        [InlineData(8)]
        [InlineData(12)]
        [InlineData(13)]
        [InlineData(23)]
        [InlineData(24)]
        public void CreateInspectionRequest_Validate_success(int frequency)
        {
            _request.Frequency = frequency;

            var result = _request.Validate(_errors);

            Assert.True(result);
            Assert.Empty(_errors);
        }

        [Theory]
        [InlineData("2021-14-12")]
        [InlineData("2021-01-02T29:01:01")]
        [InlineData("2021-01-02T11:72:01")]
        [InlineData("not a datetime")]
        public void CreateInspectionRequest_Validate_invalid_startdate(string startDate)
        {
            _request.StartDate = startDate;
            _request.EndDate = null;

           ShouldBeInvalid("StartDate", "StartDate is not a valid datetime");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData("\r")]
        public void CreateInspectionRequest_Validate_Checks_Name_required(string name)
        {
            _request.Checks[0].Name = name;

           ShouldBeInvalid("Checks[0].Name", "Checks[0].Name is required");
        }

        [Theory]
        [InlineData("2021-14-12")]
        [InlineData("2021-01-02T29:01:01")]
        [InlineData("2021-01-02T11:72:01")]
        [InlineData("not a datetime")]
        public void CreateInspectionRequest_Validate_invalid_enddate(string endDate)
        {
            _request.EndDate = endDate;

           ShouldBeInvalid("EndDate", "EndDate is not a valid datetime");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void CreateInspectionRequest_Validate_required_name(string name)
        {
            _request.Name = name;

           ShouldBeInvalid("Name", "Name is required");
        }
                
        [Theory]
        [InlineData("Click on this <a>hello</a>")]
        public void CreateInspectionRequest_Validate_invalid_name(string name)
        {
            _request.Name = name;

           ShouldBeInvalid("Name", "Name has invalid characters in it");
        }

        [Theory]
        [InlineData("Click on this <a>hello</a>")]
        public void CreateInspectionRequest_Validate_invalid_floorCode(string floorCode)
        {
            _request.FloorCode = floorCode;

           ShouldBeInvalid("FloorCode", "FloorCode has invalid characters in it");
        }

        [Fact]
        public void CreateInspectionRequest_Validate_required_floorCode()
        {
            _request.FloorCode = null;

           ShouldBeInvalid("FloorCode", "FloorCode is required");
        }

        [Theory]
        [InlineData(null)]
        public void CreateInspectionRequest_Validate_Required_Unit(SchedulingUnit? unit)
        {
            _request.FrequencyUnit = unit;

           ShouldBeInvalid("FrequencyUnit", "Frequency Unit is required");
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
