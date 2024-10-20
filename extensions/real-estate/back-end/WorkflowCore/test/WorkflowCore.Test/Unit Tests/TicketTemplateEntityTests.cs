using System;
using System.Collections.Generic;

using Xunit;

using Newtonsoft.Json;

using WorkflowCore.Models;

using Willow.Calendar;
using WorkflowCore.Entities;

namespace WorkflowCore.Test.UnitTests
{
    public class TicketTemplateEntityTests
    {
        [Fact]
        [Trait("Category", "FrequencyUnit")]
        public void TicketTemplateEnitity_ToModel_success()
        {
            var entity = new TicketTemplateEntity
            {
                Id                   = Guid.NewGuid(),
                SequenceNumber       = "1MW-BOB-1",
                Recurrence           = JsonConvert.SerializeObject(_event1),
                FloorCode            = "121",
                CustomerId           = Guid.NewGuid(),
                Status               = (int)TicketStatusEnum.Open,
                SourceType           = SourceType.Platform,
                Priority             = 4,
                Summary              = "This is a test",
                Description          = "This is a long description of a test",
                AssigneeId           = Guid.NewGuid(),
                OverdueThreshold     = "3;3",
                CategoryId           = Guid.NewGuid(),
                CategoryName         = "Jupiter",

                ReporterId           = Guid.NewGuid(),
                ReporterCompany      = "Acme Property Management",
                ReporterEmail        = "bob@acme.none",
                ReporterName         = "Bob FakeDude",
                ReporterPhone        = "555-1212",

                Assets = JsonConvert.SerializeObject(new List<TicketAsset>
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
                }),
                Tasks = JsonConvert.SerializeObject(new List<TicketTaskTemplate>
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
                })
            };

            var model = TicketTemplateEntity.MapToModel(entity);

            Assert.Equal(entity.Id,                     model.Id);
            Assert.Equal(entity.CustomerId,             model.CustomerId);
            Assert.Equal(entity.SiteId,                 model.SiteId);
            Assert.Equal(entity.FloorCode,              model.FloorCode);
            Assert.Equal(entity.SequenceNumber,         model.SequenceNumber);
            Assert.Equal(entity.Priority,               model.Priority);
            Assert.Equal(entity.Status,                 model.Status);
            Assert.Equal(entity.Summary,                model.Summary);
            Assert.Equal(entity.Description,            model.Description);
            Assert.Equal(entity.ReporterId,             model.ReporterId);
            Assert.Equal(entity.ReporterName,           model.ReporterName);
            Assert.Equal(entity.ReporterPhone,          model.ReporterPhone);
            Assert.Equal(entity.ReporterEmail,          model.ReporterEmail);
            Assert.Equal(entity.ReporterCompany,        model.ReporterCompany);
            Assert.Equal(entity.AssigneeType,           model.AssigneeType);
            Assert.Equal(entity.AssigneeId,             model.AssigneeId);
            Assert.Equal(entity.CreatedDate,            model.CreatedDate);
            Assert.Equal(entity.UpdatedDate,            model.UpdatedDate);
            Assert.Equal(entity.ClosedDate,             model.ClosedDate);
            Assert.Equal(entity.SourceType,             model.SourceType);

            Assert.Equal(entity.Recurrence,             JsonConvert.SerializeObject(model.Recurrence));
            Assert.Equal(Duration.DurationUnit.Month,   model.OverdueThreshold.UnitOfMeasure);
            Assert.Equal(3,                             model.OverdueThreshold.Units);
            Assert.Equal("Numeric Task",                model.Tasks[0].Description);
            Assert.Equal(TaskType.Numeric,              model.Tasks[0].Type);
            Assert.Equal(2,                             model.Tasks[0].DecimalPlaces);
            Assert.Equal(1000,                          model.Tasks[0].MaxValue);
            Assert.Equal(1,                             model.Tasks[0].MinValue);
            Assert.Equal("psi",                         model.Tasks[0].Unit);
            Assert.Equal("Check Task",                  model.Tasks[1].Description);
            Assert.Equal(TaskType.Checkbox,             model.Tasks[1].Type);
            Assert.Equal(entity.CategoryId,             model.CategoryId);
            Assert.Equal("Jupiter",                     model.CategoryName);
        }

        #region Sample Events

        private static Event _event1 = new Event
        {
            StartDate      = DateTime.Parse("2021-01-14T00:00:00"),
            Occurs         = Event.Recurrence.Monthly,
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
