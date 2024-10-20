using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

using Willow.DataValidation;
using Willow.Workflow;

namespace Willow.PlatformPortal.XL.UnitTests
{
    public class CreateTicketTemplateRequestTests
    {
        private List<(string Name, string Message)> _errors = new List<(string Name, string Message)>();
        private CreateTicketTemplateRequest _request;

        public CreateTicketTemplateRequestTests()
        {
            // Valid request
             _request = new CreateTicketTemplateRequest
            {
                Recurrence = new EventDto
                {
                    Name      = "bob",
                    StartDate = "2021-01-02T01:01:01",
                    Timezone  = "Pacific Standard Time",
                    Occurs    = EventDto.Recurrence.Daily
                },

                OverdueThreshold = new Calendar.Duration
                {
                   UnitOfMeasure = Calendar.Duration.DurationUnit.Day,
                   Units         = 20
                },

                Category        = "fred",

                Priority        = 2,
                Summary         = "Wilma",
                Description     = "Bedrock",
                ReporterName    = "Barney",
                ReporterPhone   = "555-555-1212",
                ReporterEmail   = "barney@rockhead.com",
                ReporterCompany = "Rockhead and Quarry"
            };

        }

        [Theory]
        [InlineData("555-555-1212")]
        [InlineData("5555551212")]
        [InlineData("555 555 1212")]
        [InlineData("(555) 555-1212")]
        [InlineData("555.555.1212")]
        public void CreateTicketTemplateRequest_Validate_success(string phone)
        {
            _request.ReporterPhone = phone;

            var result = _request.Validate(_errors);

            Assert.True(result);
            Assert.Empty(_errors);
        }

        [Theory]
        [InlineData("barney@rockhead@rockhead.com")]
        public void CreateTicketTemplateRequest_Validate_invalid_email(string emailAddress)
        {
            _request.ReporterEmail = emailAddress;

           ShouldBeInvalid("ReporterEmail", "Contact email is invalid");
        }

        [Theory]
        [InlineData("bareeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeney@rockhead.comddddddddddddddddddddddddddddddddddddddddddddddddddddddd")]
        public void CreateTicketTemplateRequest_Validate_email_too_long(string emailAddress)
        {
            _request.ReporterEmail = emailAddress;

           ShouldBeInvalid("ReporterEmail", "The field ReporterEmail must be a string with a maximum length of 64.");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public void CreateTicketTemplateRequest_Validate_email_required(string emailAddress)
        {
            _request.ReporterEmail = emailAddress;

           ShouldBeInvalid("ReporterEmail", "Contact email is required");
        }

        [Theory]
        [InlineData("2021-14-12")]
        [InlineData("2021-01-02T29:01:01")]
        [InlineData("2021-01-02T11:72:01")]
        [InlineData("not a datetime")]
        public void CreateTicketTemplateRequest_Validate_invalid_startdate(string startDate)
        {
            _request.Recurrence.StartDate = startDate;

           ShouldBeInvalid("Recurrence.StartDate", "Recurrence.StartDate is not a valid datetime");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void CreateTicketTemplateRequest_Validate_required_summary(string summary)
        {
            _request.Summary = summary;

           ShouldBeInvalid("Summary", "Summary is required");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void CreateTicketTemplateRequest_Validate_required_description(string description)
        {
            _request.Description = description;

           ShouldBeInvalid("Description", "Description is required");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void CreateTicketTemplateRequest_Validate_required_phone(string phone)
        {
            _request.ReporterPhone = phone;

           ShouldBeInvalid("ReporterPhone", "Contact number is required");
        }


        [Theory]
        [InlineData("bob")]
        [InlineData("555-555-12i9")]
        public void CreateTicketTemplateRequest_Validate_invalid_phone(string phone)
        {
            _request.ReporterPhone = phone;

           ShouldBeInvalid("ReporterPhone", "Contact number is invalid");
        }

        [Theory]
        [InlineData("click on this <a>hello</a>")]
        public void CreateTicketTemplateRequest_Validate_invalid_summary(string summary)
        {
            _request.Summary = summary;

           ShouldBeInvalid("Summary", "Summary has invalid characters in it");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void CreateTicketTemplateRequest_Validate_required_reportername(string name)
        {
            _request.ReporterName = name;

           ShouldBeInvalid("ReporterName", "Requestor name is required");
        }

        [Theory]
        [InlineData("Joe, Click on this <a>hello</a>")]
        public void CreateTicketTemplateRequest_Validate_invalid_reportername(string name)
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
