using System.Collections.Generic;
using Xunit;

using Willow.DataValidation;
using Willow.Workflow;

namespace Willow.PlatformPortal.XL.UnitTests
{
	public class UpdateTicketRequestTests
    {
        private List<(string Name, string Message)> _errors = new List<(string Name, string Message)>();
        private UpdateTicketRequest _request;

        public UpdateTicketRequestTests()
        {
            // Valid request
             _request = new UpdateTicketRequest
            {
                Priority        = 2,
                Summary         = "Wilma",
                Description     = "Bedrock",
                ReporterName    = "Barney",
                ReporterPhone   = "555-555-1212",
                ReporterEmail   = "barney@rockhead.com",
                ReporterCompany = "Rockhead and Quarry",

                StatusCode      = (int)TicketStatus.Open,
                Cause           = "bob",
                Solution        = "fred",
                Notes           = "notes",
                Template        = false
            };
        }

        [Theory]
        [InlineData("555-555-1212")]
        [InlineData("5555551212")]
        [InlineData("555 555 1212")]
        [InlineData("(555) 555-1212")]
        [InlineData("555.555.1212")]
        public void UpdateTicketRequest_Validate_success(string phone)
        {
            _request.ReporterPhone = phone;

            var result = _request.Validate(_errors);

            Assert.True(result);
            Assert.Empty(_errors);
        }

        [Theory]
        [InlineData("barney@rockhead@rockhead.com")]
        public void UpdateTicketRequest_Validate_invalid_email(string emailAddress)
        {
            _request.ReporterEmail = emailAddress;

           ShouldBeInvalid("ReporterEmail", "Contact email is invalid");
        }

        [Theory]
        [InlineData("bareeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeney@rockhead.comddddddddddddddddddddddddddddddddddddddddddddddddddddddd")]
        public void UpdateTicketRequest_Validate_email_too_long(string emailAddress)
        {
            _request.ReporterEmail = emailAddress;

           ShouldBeInvalid("ReporterEmail", "The field ReporterEmail must be a string with a maximum length of 64.");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void UpdateTicketRequest_Validate_required_summary(string summary)
        {
            _request.Summary = summary;

           ShouldBeInvalid("Summary", "Summary is required");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void UpdateTicketRequest_Validate_required_description(string description)
        {
            _request.Description = description;

           ShouldBeInvalid("Description", "Description is required");
        }


        [Theory]
        [InlineData("bob")]
        [InlineData("555-555-12i9")]
        public void UpdateTicketRequest_Validate_invalid_phone(string phone)
        {
            _request.ReporterPhone = phone;

           ShouldBeInvalid("ReporterPhone", "Contact number is invalid");
        }

        [Theory]
        [InlineData("click on this <a>hello</a>")]
        public void UpdateTicketRequest_Validate_invalid_summary(string summary)
        {
            _request.Summary = summary;

           ShouldBeInvalid("Summary", "Summary has invalid characters in it");
        }


        [Theory]
        [InlineData("Joe, Click on this <a>hello</a>")]
        public void UpdateTicketRequest_Validate_invalid_reportername(string name)
        {
            _request.ReporterName = name;

           ShouldBeInvalid("ReporterName", "ReporterName has invalid characters in it");
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
