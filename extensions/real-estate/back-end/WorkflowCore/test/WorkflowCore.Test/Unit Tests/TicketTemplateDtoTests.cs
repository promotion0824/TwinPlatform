using System;
using System.Collections.Generic;

using Moq;
using Xunit;

using WorkflowCore.Dto;
using WorkflowCore.Models;

using Willow.Calendar;
using Willow.Common;

namespace WorkflowCore.Test.UnitTests
{
    public class TicketTemplateDtoTests
    {
        private readonly Mock<IDateTimeService> _datetimeService;

        public TicketTemplateDtoTests()
        {
            _datetimeService = new Mock<IDateTimeService>();
        }

        [Fact]
        [Trait("Category", "FrequencyUnit")]
        public void TicketTemplateDto_FromModel_success()
        {
            _datetimeService.Setup(w => w.UtcNow).Returns(new DateTime(2021, 3, 3, 8, 0, 0, DateTimeKind.Utc));

            var template = new TicketTemplate
            {
                Id                   = Guid.NewGuid(),
                SequenceNumber       = "1MW-BOB-1",
                Recurrence           = EventDto.MapToModel(_event1),
                FloorCode            = "121",
                CustomerId           = Guid.NewGuid(),
                Status               = (int)TicketStatusEnum.Open,
                SourceType           = SourceType.Platform,
                Priority             = 4,
                Summary              = "This is a test",
                Description          = "This is a long description of a test",
                AssigneeId           = Guid.NewGuid(),
                OverdueThreshold     = new Duration("3;3"),
                CategoryId           = Guid.NewGuid(),
                CategoryName         = "Jupiter",

                ReporterId           = Guid.NewGuid(),
                ReporterCompany      = "Acme Property Management",
                ReporterEmail        = "bob@acme.none",
                ReporterName         = "Bob FakeDude",
                ReporterPhone        = "555-1212",

                Assets = new List<TicketAsset>
                {
                    new TicketAsset
                    {
                        AssetId = Guid.NewGuid(),
                        AssetName = "Chevy"
                    },
                    new TicketAsset
                    {
                        AssetId = Guid.NewGuid(),
                        AssetName = "Pontiac"
                    }
                },
                Tasks = new List<TicketTaskTemplate>
                {
                    new TicketTaskTemplate
                    {
                        Description = "Numeric Task",
                        Type = TaskType.Numeric,
                        DecimalPlaces = 2,
                        MaxValue = 1000,
                        MinValue = 1,
                        Unit = "psi"
                    },
                    new TicketTaskTemplate
                    {
                        Description = "Check Task",
                        Type = TaskType.Checkbox
                    }
                }
            };

            var dto = TicketTemplateDto.MapFromModel(template, null, _datetimeService.Object);

            Assert.Equal(template.Id,                 dto.Id);
            Assert.Equal(template.CustomerId,         dto.CustomerId);
            Assert.Equal(template.SiteId,             dto.SiteId);
            Assert.Equal(template.FloorCode,          dto.FloorCode);
            Assert.Equal(template.SequenceNumber,     dto.SequenceNumber);
            Assert.Equal(template.Priority,           dto.Priority);
            Assert.Equal(template.Status,             dto.Status);
            Assert.Equal(template.Summary,            dto.Summary);
            Assert.Equal(template.Description,        dto.Description);
            Assert.Equal(template.ReporterId,         dto.ReporterId);
            Assert.Equal(template.ReporterName,       dto.ReporterName);
            Assert.Equal(template.ReporterPhone,      dto.ReporterPhone);
            Assert.Equal(template.ReporterEmail,      dto.ReporterEmail);
            Assert.Equal(template.ReporterCompany,    dto.ReporterCompany);
            Assert.Equal(template.AssigneeType,       dto.AssigneeType);
            Assert.Equal(template.AssigneeId,         dto.AssigneeId);
            Assert.Equal(template.CreatedDate,        dto.CreatedDate);
            Assert.Equal(template.UpdatedDate,        dto.UpdatedDate);
            Assert.Equal(template.ClosedDate,         dto.ClosedDate);
            Assert.Equal(template.SourceType,         dto.SourceType);

            Assert.Equal(template.Recurrence.StartDate,         DateTime.Parse(dto.Recurrence.StartDate));           
            Assert.Equal(template.Recurrence.Timezone,          dto.Recurrence.Timezone);          
            Assert.Equal(template.Recurrence.Name,              dto.Recurrence.Name);           
            Assert.Equal(template.Recurrence.Timezone,          dto.Recurrence.Timezone);          
            Assert.Equal(template.Recurrence.DayOccurrences,    dto.Recurrence.DayOccurrences);    
            Assert.Equal(template.Recurrence.Days,              dto.Recurrence.Days);              
            Assert.Equal(template.Recurrence.MaxOccurrences,    dto.Recurrence.MaxOccurrences);     
            Assert.Equal(template.Recurrence.Interval,          dto.Recurrence.Interval);          

            Assert.Equal("2021-04-07T00:00:00",                 dto.NextTicketDate);
            Assert.Equal(template.OverdueThreshold.UnitOfMeasure,   dto.OverdueThreshold.UnitOfMeasure);
            Assert.Equal(template.OverdueThreshold.Units,   dto.OverdueThreshold.Units);
            Assert.Equal(template.CategoryId,         dto.CategoryId);
            Assert.Equal("Jupiter",                   dto.Category);
            Assert.Equal("Numeric Task",              dto.Tasks[0].Description);
            Assert.Equal(TaskType.Numeric,            dto.Tasks[0].Type);
            Assert.Equal(2,                           dto.Tasks[0].DecimalPlaces);
            Assert.Equal(1000,                        dto.Tasks[0].MaxValue);
            Assert.Equal(1,                           dto.Tasks[0].MinValue);
            Assert.Equal("psi",                       dto.Tasks[0].Unit);
            Assert.Equal("Check Task",                dto.Tasks[1].Description);
            Assert.Equal(TaskType.Checkbox,           dto.Tasks[1].Type);
        }

        #region Sample Events

        private static EventDto _event1 = new EventDto
        {
            StartDate      =  "2021-01-14T00:00:00",
            Occurs         = Event.Recurrence.Monthly,
            Timezone       = "Pacific Standard Time",
            DayOccurrences = new List<Event.DayOccurrence>
            {
                new Event.DayOccurrence
                {
                    Ordinal = 1,
                    DayOfWeek = DayOfWeek.Wednesday
                }
            }
        };

        #endregion
    }
}
