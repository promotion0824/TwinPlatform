using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Newtonsoft.Json;
using WorkflowCore.Dto;
using WorkflowCore.Models;
using WorkflowCore.Services;
using WorkflowCore.Repository;
using Microsoft.Extensions.Logging;
using Willow.Calendar;
using Willow.Scheduler;
using System.Reflection;
using Willow.Common;
using WorkflowCore.Services.Apis;
using Willow.ExceptionHandling.Exceptions;


namespace WorkflowCore.Test.UnitTests
{
    public class TicketTemplateServiceTests
    {
        private readonly ITicketTemplateService _svc;

        private readonly Mock<ITicketTemplateRepository> _repo;
        private readonly Mock<IDateTimeService> _datetimeService;
        private readonly Mock<IWorkflowService> _workflowService;
        private readonly Mock<IDigitalTwinServiceApi> _digitalTwinApi;
        private readonly Mock<IInsightServiceApi> _insightApi;

        private readonly Mock<ISchedulerRepository> _scheduleRepo;
        private readonly Mock<ILogger<SchedulerService>> _logger;
        private readonly SchedulerService _schedulerService;

        private static Guid templateId1   = Guid.NewGuid();
        private static Guid templateId2   = Guid.NewGuid();
        private static Guid templateId3   = Guid.NewGuid();
        private static Guid templateId4   = Guid.NewGuid();
        private static Guid templateId5   = Guid.NewGuid();
        private static Guid assetId1      = Guid.NewGuid();
        private static Guid assetId2      = Guid.NewGuid();
        private static Guid assetId3      = Guid.NewGuid();
        private static Guid attachmentId1 = Guid.NewGuid();
        private static Guid attachmentId2 = Guid.NewGuid();
        private static Guid taskId1       = Guid.NewGuid();
        private static Guid taskId2       = Guid.NewGuid();
                                   
        private static Guid categoryId1   = Guid.NewGuid();
        private static Guid categoryId2   = Guid.NewGuid();
        private static Guid customerId    = Guid.NewGuid();
        private static Guid siteId        = Guid.NewGuid();
        private static Guid assignee      = Guid.NewGuid();
        private static Guid reporterId    = Guid.NewGuid();
        private static DateTime utcNow    = DateTimeOffset.Parse("2021-03-03T00:00:00").Date;

        private List<TicketTemplate> _templates = new List<TicketTemplate>();
        private List<Ticket> _tickets = new List<Ticket>();

        private TicketTemplate _template1;
        private TicketTemplate _template2;
        private TicketTemplate _template3;
        private TicketTemplate _template4;
        private TicketTemplate _template5;

        private readonly Guid[] _templateIds = new Guid[] { templateId1, templateId2, templateId3, templateId4, templateId5};
        private readonly TicketTemplate[] _sampleTemplates;

        public TicketTemplateServiceTests()
        {
            _repo = new Mock<ITicketTemplateRepository>();
            _datetimeService = new Mock<IDateTimeService>();
            _workflowService = new Mock<IWorkflowService>();
            _digitalTwinApi = new Mock<IDigitalTwinServiceApi>();
            _insightApi = new Mock<IInsightServiceApi>();
            _svc = new TicketTemplateService(_datetimeService.Object, _repo.Object, _workflowService.Object, _digitalTwinApi.Object, 1);
            
            #region Sample Templates

            _template1 = new TicketTemplate 
            {
                Id               = templateId1,
                Recurrence       = EventDto.MapToModel(_event1),
                OverdueThreshold = new Duration("3;3"),
                FloorCode        = "121",
                CustomerId       = customerId,
                SiteId           = siteId,
                Status           = (int)TicketStatusEnum.Open,
                SourceType       = SourceType.Platform,
                Priority         = 4,
                Summary          = "This is a test",
                Description      = "This is a long description of a test",
                AssigneeId       = assignee,
                AssigneeType     = AssigneeType.CustomerUser,
                SequenceNumber   = "BOB-S-1",
                CategoryId       = categoryId1,
                CategoryName     = "Jupiter",

                ReporterId       = reporterId,
                ReporterCompany  = "Acme Property Management",
                ReporterEmail    = "bob@acme.none",
                ReporterName     = "Bob FakeDude",
                ReporterPhone    = "555-1212",

                Assets = new List<TicketAsset>
                {
                    new TicketAsset
                    {
                        Id = assetId1,
                        AssetId = assetId1,
                        AssetName = "Chevy"
                    },
                    new TicketAsset
                    {
                        Id = assetId2,
                        AssetId = assetId2,
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

            _template2 = new TicketTemplate 
            {
                Id          = templateId2,
                Recurrence  = EventDto.MapToModel(_event1),
                OverdueThreshold = new Duration("3;3"),
                CustomerId  = customerId,
                SiteId      = siteId,
                Status      = (int)TicketStatusEnum.Open,
                SourceType  = SourceType.Platform,
                Priority    = 4,
                Summary     = "This is a test",
                Description = "This is a long description of a test",
                AssigneeId  = assignee,
                SequenceNumber = "BOB-S-2",
                CategoryId       = categoryId2,
                CategoryName     = "Saturn",

                ReporterId      = reporterId,
                ReporterCompany = "Acme Property Management",
                ReporterEmail   = "bob@acme.none",
                ReporterName    = "Bob FakeDude",
                ReporterPhone   = "555-1212",

                Assets = new List<TicketAsset>
                {
                    new TicketAsset
                    {
                        Id = assetId1,
                        AssetId = assetId1,
                        AssetName = "Chevy"
                    },
                    new TicketAsset
                    {
                        Id = assetId2,
                        AssetId = assetId2,
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
                },

                Attachments = new List<TicketAttachment>
                {
                    new TicketAttachment
                    {
                        Id = attachmentId1,
                        TicketId = templateId2,
                        Type = AttachmentType.File,
                        FileName = "blah1.doc",
                        CreatedDate = utcNow.AddDays(-10)
                    },
                    new TicketAttachment
                    {
                        Id = attachmentId2,
                        TicketId = templateId2,
                        Type = AttachmentType.File,
                        FileName = "blah2.doc",
                        CreatedDate = utcNow.AddDays(-10)
                    }
                }
            };

            _template3 = new TicketTemplate 
            {
                Id          = templateId3,
                Recurrence  = EventDto.MapToModel(_event3),
                OverdueThreshold = new Duration("3;3"),
                CustomerId  = customerId,
                SiteId      = siteId,
                Status      = (int)TicketStatusEnum.Open,
                SourceType  = SourceType.Platform,
                Priority    = 4,
                Summary     = "This is a test",
                Description = "This is a long description of a test",
                AssigneeId  = assignee,
                SequenceNumber = "BOB-S-2",
                CategoryId       = categoryId2,
                CategoryName     = "Saturn",

                ReporterId      = reporterId,
                ReporterCompany = "Acme Property Management",
                ReporterEmail   = "bob@acme.none",
                ReporterName    = "Bob FakeDude",
                ReporterPhone   = "555-1212",

                Assets = new List<TicketAsset>
                {
                    new TicketAsset
                    {
                        Id = assetId1,
                        AssetId = assetId1,
                        AssetName = "Chevy"
                    },
                    new TicketAsset
                    {
                        Id = assetId2,
                        AssetId = assetId2,
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
                },

                Attachments = new List<TicketAttachment>
                {
                    new TicketAttachment
                    {
                        Id = attachmentId1,
                        TicketId = templateId2,
                        Type = AttachmentType.File,
                        FileName = "blah1.doc",
                        CreatedDate = utcNow.AddDays(-10)
                    },
                    new TicketAttachment
                    {
                        Id = attachmentId2,
                        TicketId = templateId2,
                        Type = AttachmentType.File,
                        FileName = "blah2.doc",
                        CreatedDate = utcNow.AddDays(-10)
                    }
                }
            };

            _template4 = new TicketTemplate 
            {
                Id               = templateId4,
                Recurrence       = EventDto.MapToModel(_event4),
                OverdueThreshold = new Duration("3;3"),
                FloorCode        = "121",
                CustomerId       = customerId,
                SiteId           = siteId,
                Status           = (int)TicketStatusEnum.Open,
                SourceType       = SourceType.Platform,
                Priority         = 4,
                Summary          = "This is a test",
                Description      = "This is a long description of a test",
                AssigneeId       = assignee,
                AssigneeType     = AssigneeType.CustomerUser,
                SequenceNumber   = "BOB-S-1",
                CategoryId       = categoryId1,
                CategoryName     = "Jupiter",

                ReporterId       = reporterId,
                ReporterCompany  = "Acme Property Management",
                ReporterEmail    = "bob@acme.none",
                ReporterName     = "Bob FakeDude",
                ReporterPhone    = "555-1212",

                Assets = new List<TicketAsset>
                {
                    new TicketAsset
                    {
                        Id = assetId1,
                        AssetId = assetId1,
                        AssetName = "Chevy"
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

            _template5 = new TicketTemplate 
            {
                Id               = templateId5,
                Recurrence       = EventDto.MapToModel(_event5),
                OverdueThreshold = new Duration("3;3"),
                FloorCode        = "121",
                CustomerId       = customerId,
                SiteId           = siteId,
                Status           = (int)TicketStatusEnum.Open,
                SourceType       = SourceType.Platform,
                Priority         = 4,
                Summary          = "This is a test",
                Description      = "This is a long description of a test",
                AssigneeId       = assignee,
                AssigneeType     = AssigneeType.CustomerUser,
                SequenceNumber   = "BOB-S-1",
                CategoryId       = categoryId1,
                CategoryName     = "Jupiter",

                ReporterId       = reporterId,
                ReporterCompany  = "Acme Property Management",
                ReporterEmail    = "bob@acme.none",
                ReporterName     = "Bob FakeDude",
                ReporterPhone    = "555-1212",

                Assets = new List<TicketAsset>
                {
                    new TicketAsset
                    {
                        Id = assetId1,
                        AssetId = assetId1,
                        AssetName = "Chevy"
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

             _sampleTemplates = new TicketTemplate[] { _template1,  _template2,  _template3,  _template4,  _template5};

            #endregion

            _repo.Setup(w => w.GetTicketTemplate(templateId1)).ReturnsAsync(_template1);
            _repo.Setup(w => w.GetTicketTemplate(templateId2)).ReturnsAsync(_template2);
            _repo.Setup(w => w.GetTicketTemplate(templateId3)).ReturnsAsync(_template3);
            _repo.Setup(w => w.GetTicketTemplate(templateId4)).ReturnsAsync(_template4);
            _repo.Setup(w => w.GetTicketTemplate(templateId5)).ReturnsAsync(_template5);

            _repo.Setup(w => w.CreateTicketTemplate(It.IsAny<TicketTemplate>())).Callback<TicketTemplate>( t=> 
            { 
                t.CategoryName = t.CategoryId == categoryId1 ? "Jupiter" : "Saturn"; 
                _templates.Add(t); 
            });

            _workflowService.Setup(w => w.CreateTicket(It.IsAny<List<Ticket>>(),It.IsAny<Guid>(), "en")).Callback<List<Ticket>,Guid, string>( (t,s, l) => _tickets.Add(t.First()) );

            _scheduleRepo   = new Mock<ISchedulerRepository>();
            _logger = new Mock<ILogger<SchedulerService>>();
           
            _schedulerService = new SchedulerService(_scheduleRepo.Object, _logger.Object, new Dictionary<string, IScheduleRecipient> { {"WorkflowCore:TicketTemplate", _svc } }, 1);
       }

        [Theory]
        [InlineData("2021-01-10T00:00:00", 0, 0)]
        [InlineData("2021-01-14T00:00:00", 0, 0)]
        [InlineData("2021-01-11T00:00:00", 7, 2)]
        [InlineData("2021-01-14T00:00:00", 1, 0)]
        [InlineData("2021-01-14T08:00:00", 1, 2)]
        [InlineData("2021-01-14T00:00:00", 7, 2)]
        [InlineData("2021-01-14T08:00:00", 7, 2)]
        [InlineData("2021-01-01T00:00:00", 7, 0)]
        [Trait("Category", "FrequencyUnit")]
        public async Task TicketTemplateService_CreateTicketTemplate_success(string today, int advance, int expectedTickets)
        {
            await CreateTicketTemplate(today, advance, expectedTickets, _event1, _template1);
        }

        [Trait("Category", "FrequencyUnit")]
        [Theory]
        [InlineData("2021-06-15T07:22:00", "2021-06-16T08:00:00", 1, 0, 4184)]
        [InlineData("2021-06-15T14:22:00", "2021-06-16T00:00:00", 1, 1, 4185)]
        public async Task TicketTemplateService_CreateTicketTemplate_success2(string today, string scheduledHit, int advance, int expectedTickets, int occurrence)
        {
            await CreateTicketTemplate(today, advance, expectedTickets, _event4, _template4);

            var hitDate = DateTime.Parse(scheduledHit);

            foreach (var asset in _template4.Assets)
            {
                _workflowService.Setup(wf => wf.TicketOccurrenceExists(_templates[0].Id, asset.AssetId, occurrence)).ReturnsAsync(true);
                _workflowService.Setup(wf => wf.TicketOccurrenceExists(_templates[0].Id, asset.AssetId.ToString(), occurrence)).ReturnsAsync(true);
            }

            await _svc.PerformScheduleHit(new ScheduleHit
            {
                ScheduleId = Guid.NewGuid(),
                OwnerId    = _templates[0].Id,
                HitDate    = hitDate,
                EventName  = "Test",
            }, "en");

            _workflowService.Verify( wf=> wf.CreateTicket(It.IsAny<List<Ticket>>(), It.IsAny<Guid>(), "en"), Times.Once);

            Assert.Single(_templates);
        }
        
        [Trait("Category", "FrequencyUnit")]
        [Theory]
        [InlineData("2021-06-18T05:13:27", "2021-06-18T16:00:00", "2021-06-19T00:00:00", 1, 0, 137)]
        public async Task TicketTemplateService_CreateTicketTemplate_success3(string today, string scheduledHit, string scheduledHit2, int advance, int expectedTickets, int occurrence)
        {
            await CreateTicketTemplate(today, advance, expectedTickets, _event5, _template5);

            _repo.Setup( r=> r.GetTicketTemplate(_template5.Id)).ReturnsAsync(_template5);

            SetupTicketOccurrenceExists(_template5.Id, _template5.Assets, occurrence, false);

            _scheduleRepo.Setup(r=> r.GetSchedules()).ReturnsAsync(new List<Schedule> { new Schedule { OwnerId = _template5.Id, Recurrence = JsonConvert.SerializeObject(EventDto.MapToModel(_event5)), RecipientClient = "WorkflowCore", Recipient = "TicketTemplate" } } );

            await _schedulerService.CheckSchedules(DateTime.Parse(scheduledHit), "en");

            _workflowService.Verify( wf=> wf.CreateTicket(It.IsAny<List<Ticket>>(), It.IsAny<Guid>(), "en"), Times.Once);

            SetupTicketOccurrenceExists(_template5.Id, _template5.Assets, occurrence+1, true);

            await _schedulerService.CheckSchedules(DateTime.Parse(scheduledHit2), "en");

            _workflowService.Verify( wf=> wf.CreateTicket(It.IsAny<List<Ticket>>(), It.IsAny<Guid>(), "en"), Times.Once);

            Assert.Single(_templates);
        }

        private void SetupTicketOccurrenceExists(Guid templateId, IList<TicketAsset> assets, int occurrence, bool returns)
        {
            foreach (var asset in assets)
            {
                _workflowService.Setup(wf => wf.TicketOccurrenceExists(templateId, asset.AssetId, occurrence)).ReturnsAsync(returns);
                _workflowService.Setup(wf => wf.TicketOccurrenceExists(templateId, asset.AssetId.ToString(), occurrence)).ReturnsAsync(returns);
            }
        }

        #region UpdateTicketTemplate

        [Trait("Category", "FrequencyUnit")]
        [Theory]
        [InlineData("2022-05-02T01:00:00", "2022-04-28T11:00:00")] // starts in past
        [InlineData("2022-05-02T01:00:00", "2022-05-02T10:40:00")] // starts today (20 minutes ago)
        [InlineData("2022-05-02T01:00:00", "2022-05-02T10:59:59")] // starts today (1 sec ago)
        [InlineData("2022-05-02T01:00:00", "2022-05-01T11:00:00")] // started yesterday
        public async Task TicketTemplateService_UpdateTicketTemplate_AddTwoAssets(string utcToday, string startDate)
        {
            var templateId = Guid.NewGuid();

            var evt = new EventDto
            {
                StartDate      = startDate,
                Occurs         = Event.Recurrence.Monthly,
                Interval       = 6,
                Timezone       = "AUS Eastern Standard Time",
                Days           = new List<int> { 20 }
            };

            var template = new TicketTemplate 
            {
                Id               = templateId,
                Recurrence       = EventDto.MapToModel(evt),
                OverdueThreshold = new Duration("3;3"),
                FloorCode        = "121",
                CustomerId       = customerId,
                SiteId           = siteId,
                Status           = (int)TicketStatusEnum.Open,
                SourceType       = SourceType.Platform,
                Priority         = 4,
                Summary          = "This is a test",
                Description      = "This is a long description of a test",
                AssigneeId       = assignee,
                AssigneeType     = AssigneeType.CustomerUser,
                SequenceNumber   = "BOB-S-1",
                CategoryId       = categoryId1,
                CategoryName     = "Jupiter",

                ReporterId       = reporterId,
                ReporterCompany  = "Acme Property Management",
                ReporterEmail    = "bob@acme.none",
                ReporterName     = "Bob FakeDude",
                ReporterPhone    = "555-1212",

                Assets = new List<TicketAsset>
                {
                    new TicketAsset
                    {
                        AssetId = assetId3, 
                        AssetName = "Studebaker"
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

            _datetimeService.Setup(w => w.UtcNow).Returns(DateTime.Parse(utcToday));
            _repo.Setup(w => w.GetTicketTemplate(templateId)).ReturnsAsync(template);

            _digitalTwinApi.Setup(x => x.GetTwinIdsByUniqueIdsAsync(It.IsAny<Guid>(), It.IsAny<IEnumerable<Guid>>()))
                .ReturnsAsync((Guid sId, IEnumerable<Guid> aIds) => aIds.Select(x => new TwinIdDto { Id = x.ToString(), UniqueId = x.ToString() }).ToList());

            var assets = new List<TicketAsset>
            {
                new TicketAsset
                {
                    AssetId = assetId1, // This one is new
                    AssetName = "Chevy"
                },
                new TicketAsset
                {
                    AssetId = assetId2, // This one is new
                    AssetName = "Pontiac"
                },
                new TicketAsset
                {
                    AssetId = assetId3,
                    AssetName = "Studebaker"
                }
            };

            var request = new Controllers.Request.UpdateTicketTemplateRequest() 
            {
                PerformScheduleHitOnAddedAssets = true,
                Assets = assets,
            };

            await _svc.UpdateTicketTemplate(templateId, request, "en");

            _workflowService.Verify(wf => wf.CreateTicket(It.IsAny<List<Ticket>>(), It.IsAny<Guid>(), "en"), Times.Exactly(2));

            Assert.Equal(2, _tickets.Count);

            Assert.Equal(assetId1.ToString(), _tickets[0].IssueId?.ToString() ?? _tickets[0].TwinId);
            Assert.Equal("Chevy", _tickets[0].IssueName);
            Assert.Equal(IssueType.Asset, _tickets[0].IssueType);

            Assert.Equal(assetId2.ToString(), _tickets[1].IssueId?.ToString() ?? _tickets[1].TwinId);
            Assert.Equal("Pontiac", _tickets[1].IssueName);
            Assert.Equal(IssueType.Asset, _tickets[1].IssueType);

            SetupTicketOccurrenceExists(templateId, assets, DateTime.Parse(startDate).AddDays(-1).Daydex(), true);

            await _svc.UpdateTicketTemplate(templateId, request, "en");

            _workflowService.Verify(wf => wf.CreateTicket(It.IsAny<List<Ticket>>(), It.IsAny<Guid>(), "en"), Times.Exactly(2));
        }

        [Trait("Category", "FrequencyUnit")]
        [Fact]
        public async Task TicketTemplateService_UpdateTicketTemplate_AddOneAssetPerformHitOnAddedAssetsFalse()
        {
            await CreateTicketTemplate("2021-06-20T20:00:00", 1, 0, _event5, _template5);

            await _svc.UpdateTicketTemplate(_templates[0].Id, new Controllers.Request.UpdateTicketTemplateRequest()
            {
                PerformScheduleHitOnAddedAssets = false,
                Assets = new List<TicketAsset>
                {
                    new TicketAsset
                    {
                        AssetId = assetId1,
                        AssetName = "Chevy"
                    },
                    new TicketAsset
                    {
                        AssetId = assetId2,
                        AssetName = "Pontiac"
                    }
                },
            }, "en");

            _workflowService.Verify(wf => wf.CreateTicket(It.IsAny<List<Ticket>>(), It.IsAny<Guid>(), "en"), Times.Never);
        }

        [Trait("Category", "FrequencyUnit")]
        [Fact]
        public async Task TicketTemplateService_UpdateTicketTemplate_AddTwoAsset()
        {
            await CreateTicketTemplate("2021-06-20T20:00:00", 1, 0, _event5, _template5);

            await _svc.UpdateTicketTemplate(_templates[0].Id, new Controllers.Request.UpdateTicketTemplateRequest()
            {
                PerformScheduleHitOnAddedAssets = true,
                Assets = new List<TicketAsset>
                {
                    new TicketAsset
                    {
                        AssetId = assetId1,
                        AssetName = "Chevy"
                    },
                    new TicketAsset
                    {
                        AssetId = assetId2,
                        AssetName = "Pontiac"
                    },
                    new TicketAsset
                    {
                        AssetId = assetId3,
                        AssetName = "Toyota"
                    }
                },
            }, "en");

            _workflowService.Verify(wf => wf.CreateTicket(It.IsAny<List<Ticket>>(), It.IsAny<Guid>(), "en"), Times.Exactly(2));

            Assert.Equal(2, _tickets.Count);

            Assert.Equal(_templates[0].Id, _tickets[0].TemplateId);
            Assert.Equal(assetId2.ToString(), _tickets[0].IssueId?.ToString() ?? _tickets[0].TwinId);
            Assert.Equal("Pontiac", _tickets[0].IssueName);
            Assert.Equal(IssueType.Asset, _tickets[0].IssueType);

            Assert.Equal(_templates[0].Id, _tickets[1].TemplateId);
            Assert.Equal(assetId3.ToString(), _tickets[1].IssueId?.ToString() ?? _tickets[1].TwinId);
            Assert.Equal("Toyota", _tickets[1].IssueName);
            Assert.Equal(IssueType.Asset, _tickets[1].IssueType);
        }

        [Trait("Category", "FrequencyUnit")]
        [Fact]
        public async Task TicketTemplateService_UpdateTicketTemplate_AddNoAsset()
        {
            await CreateTicketTemplate("2021-06-20T20:00:00", 1, 0, _event5, _template5);

            await _svc.UpdateTicketTemplate(_templates[0].Id, new Controllers.Request.UpdateTicketTemplateRequest()
            {
                PerformScheduleHitOnAddedAssets = true,
            }, "en");

            _workflowService.Verify(wf => wf.CreateTicket(It.IsAny<List<Ticket>>(), It.IsAny<Guid>(), "en"), Times.Never);
        }

        #endregion

        private async Task CreateTicketTemplate(string today, int advance, int expectedTickets, EventDto evt, TicketTemplate template)
        {
            _templates.Clear();

            var sequenceNumberPrefix = "BOB";
            var utcToday = DateTime.Parse(today);

            var svc = new TicketTemplateService(_datetimeService.Object, _repo.Object, _workflowService.Object, _digitalTwinApi.Object, advance);

            _workflowService.Setup(w => w.GenerateSequenceNumber(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync($"{sequenceNumberPrefix}-S-1");
            _datetimeService.Setup(w => w.UtcNow).Returns(utcToday);
            _repo.Setup(w => w.GetTicketTemplate(It.IsAny<Guid>())).ReturnsAsync( (Guid id)=> _templates.Where( t=> t.Id == id).FirstOrDefault() );

            _digitalTwinApi.Setup(x => x.GetTwinIdsByUniqueIdsAsync(It.IsAny<Guid>(), It.IsAny<IEnumerable<Guid>>()))
                    .ReturnsAsync((Guid sId, IEnumerable<Guid> aIds) => aIds.Select(x => new TwinIdDto { Id = x.ToString(), UniqueId = x.ToString() }).ToList());
            _insightApi.Setup(c =>
                    c.UpdateInsightStatusAsync(It.IsAny<Guid>(), It.IsAny<BatchUpdateInsightStatusRequest>()))
                .Returns(Task.CompletedTask);
            await svc.CreateTicketTemplate(siteId, new Controllers.Request.CreateTicketTemplateRequest
            {
                SequenceNumberPrefix = sequenceNumberPrefix,
                Recurrence           = evt,
                FloorCode            = "121",
                CustomerId           = customerId,
                Status               = (int)TicketStatusEnum.Open,
                SourceType           = SourceType.Platform,
                Priority             = 4,
                Summary              = "This is a test",
                Description          = "This is a long description of a test",
                AssigneeId           = assignee,
                AssigneeType         = AssigneeType.CustomerUser,
                OverdueThreshold     = new Duration("3;3"),
                CategoryId           = categoryId1,

                ReporterId           = reporterId,
                ReporterCompany      = "Acme Property Management",
                ReporterEmail        = "bob@acme.none",
                ReporterName         = "Bob FakeDude",
                ReporterPhone        = "555-1212",

                Assets               = template.Assets,
                Tasks                = template.Tasks
            }, "en");

            Assert.Single(_templates);

            var result = _templates[0];
             
            Assert.True(result.GetType().IsEquivalentTo(template.GetType()));

            Assert.NotEqual(result.Id,                Guid.Empty);
            Assert.Equal(template.CustomerId,         result.CustomerId);
            Assert.Equal(template.SiteId,             result.SiteId);
            Assert.Equal(template.FloorCode,          result.FloorCode);
            Assert.Equal(template.SequenceNumber,     result.SequenceNumber);
            Assert.Equal(template.Priority,           result.Priority);
            Assert.Equal(template.Status,             result.Status);
            Assert.Equal(template.Summary,            result.Summary);
            Assert.Equal(template.Description,        result.Description);
            Assert.Equal(template.ReporterId,         result.ReporterId);
            Assert.Equal(template.ReporterName,       result.ReporterName);
            Assert.Equal(template.ReporterPhone,      result.ReporterPhone);
            Assert.Equal(template.ReporterEmail,      result.ReporterEmail);
            Assert.Equal(template.ReporterCompany,    result.ReporterCompany);
            Assert.Equal(template.AssigneeType,       result.AssigneeType);
            Assert.Equal(template.AssigneeId,         result.AssigneeId);
            Assert.Equal(utcToday,                    result.CreatedDate);
            Assert.Equal(utcToday,                    result.UpdatedDate);
            Assert.Equal(template.ClosedDate,         result.ClosedDate);
            Assert.Equal(template.SourceType,         result.SourceType);
            Assert.Equal(JsonConvert.SerializeObject(template.Recurrence), JsonConvert.SerializeObject(result.Recurrence));
            Assert.Equal(template.OverdueThreshold.UnitOfMeasure,   result.OverdueThreshold.UnitOfMeasure);
            Assert.Equal(template.OverdueThreshold.Units,   result.OverdueThreshold.Units);
            Assert.Equal(template.CategoryId,         result.CategoryId);
            Assert.Equal("Jupiter",                   result.CategoryName);
            Assert.Equal("Numeric Task",              result.Tasks[0].Description);
            Assert.Equal(TaskType.Numeric,            result.Tasks[0].Type);
            Assert.Equal(2,                           result.Tasks[0].DecimalPlaces);
            Assert.Equal(1000,                        result.Tasks[0].MaxValue);
            Assert.Equal(1,                           result.Tasks[0].MinValue);
            Assert.Equal("psi",                       result.Tasks[0].Unit);
            Assert.Equal("Check Task",                result.Tasks[1].Description);
            Assert.Equal(TaskType.Checkbox,           result.Tasks[1].Type);

            var startDate = DateTime.Parse(evt.StartDate);
            var siteNow   = DateTime.Parse(today).InTimeZone(evt.Timezone).Date;

            _workflowService.Verify( s=> s.GenerateSequenceNumber("BOB", "S"), Times.Exactly(expectedTickets + 1));
            _workflowService.Verify( s=> s.CreateTicket(It.IsAny<List<Ticket>>(), It.IsAny<Guid>(), "en"), Times.Exactly(expectedTickets));
        }

        [Fact]
        [Trait("Category", "FrequencyUnit")]
        public async Task TicketTemplateService_PerformScheduleHit_notfound()
        {
            await Assert.ThrowsAsync<NotFoundException>( async ()=> await _svc.PerformScheduleHit(new ScheduleHit
            {
                ScheduleId = Guid.NewGuid(),
                OwnerId   = Guid.NewGuid(),
                HitDate    = DateTime.Parse("2021-03-03T00:00:00"),
                EventName  = "Test",
            }, "en"));
        }

        [Fact]
        [Trait("Category", "FrequencyUnit")]
        public async Task TicketTemplateService_PerformScheduleHit_success()
        {
            var sequenceNumberPrefix = "BOB";
            _workflowService.SetupSequence(w => w.GenerateSequenceNumber(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync($"{sequenceNumberPrefix}-S-1").ReturnsAsync($"{sequenceNumberPrefix}-S-2");
            _datetimeService.Setup(w => w.UtcNow).Returns(DateTime.Parse("2021-03-03T00:00:00"));

            _digitalTwinApi.Setup(x => x.GetTwinIdsByUniqueIdsAsync(It.IsAny<Guid>(), It.IsAny<IEnumerable<Guid>>()))
                .ReturnsAsync((Guid sId, IEnumerable<Guid> aIds) => aIds.Select(x => new TwinIdDto { Id = x.ToString(), UniqueId = x.ToString() }).ToList());

            await _svc.PerformScheduleHit(new ScheduleHit
            {
                ScheduleId = Guid.NewGuid(),
                OwnerId   = templateId1,
                HitDate    = DateTime.Parse("2021-03-03T00:00:00"),
                EventName  = "Test"
            }, "en");

            _workflowService.Verify(x => x.CreateTicket(It.IsAny<List<Ticket>>(), It.IsAny<Guid>(), "en"), Times.Exactly(2));

            Assert.Equal(2, _tickets.Count);

            Assert.Equal(2, _tickets[0].Tasks.Count);
            Assert.Empty(_tickets[0].Attachments ?? new List<TicketAttachment>());
            
            Assert.Equal(customerId,            _tickets[0].CustomerId);
            Assert.Equal(siteId,                _tickets[0].SiteId);
            Assert.Equal((int)TicketStatusEnum.Open,     _tickets[0].Status);
            Assert.Equal(4,                     _tickets[0].Priority);
            Assert.Equal(assignee,              _tickets[0].AssigneeId);
            Assert.Equal(AssigneeType.CustomerUser, _tickets[0].AssigneeType);
            Assert.Equal("121",                 _tickets[0].FloorCode);
            Assert.Equal(SourceType.Platform,   _tickets[0].SourceType);
            Assert.Equal("This is a test",      _tickets[0].Summary);
            Assert.Equal("This is a long description of a test", _tickets[0].Description);

            Assert.Equal(templateId1,           _tickets[0].TemplateId);
            Assert.Equal(4080,                  _tickets[0].Occurrence);

            Assert.Equal("BOB-S-1",             _tickets[0].SequenceNumber);
            Assert.Equal("BOB-S-2",             _tickets[1].SequenceNumber);

            Assert.Equal(DateTime.Parse("2021-03-03T00:00:00"), _tickets[0].ScheduledDate);
            Assert.Equal(DateTime.Parse("2021-06-03T00:00:00"), _tickets[0].DueDate);

            Assert.Equal(reporterId,                  _tickets[0].ReporterId);
            Assert.Equal("Acme Property Management",  _tickets[0].ReporterCompany);
            Assert.Equal("bob@acme.none",             _tickets[0].ReporterEmail);  
            Assert.Equal("Bob FakeDude",              _tickets[0].ReporterName);   
            Assert.Equal("555-1212",                  _tickets[0].ReporterPhone); 

            Assert.Equal(assetId1.ToString(),          _tickets[0].IssueId?.ToString() ?? _tickets[0].TwinId);
            Assert.Equal("Chevy",                     _tickets[0].IssueName);
            Assert.Equal(IssueType.Asset,             _tickets[0].IssueType);
            Assert.Equal(categoryId1,                 _tickets[0].CategoryId);

            Assert.Equal("Numeric Task",              _tickets[0].Tasks[0].TaskName);
            Assert.Equal("Check Task",                _tickets[0].Tasks[1].TaskName);
            Assert.Equal(1,                           _tickets[0].Tasks[0].MinValue);
            Assert.Equal(1000,                        _tickets[0].Tasks[0].MaxValue);
            Assert.Equal(2,                           _tickets[0].Tasks[0].DecimalPlaces);
            Assert.Equal("psi",                       _tickets[0].Tasks[0].Unit);
            Assert.Equal(TaskType.Numeric,            _tickets[0].Tasks[0].Type);
            Assert.Equal(1,                           _tickets[0].Tasks[0].Order);
            Assert.Equal(2,                           _tickets[0].Tasks[1].Order);
            Assert.Equal(TaskType.Checkbox,           _tickets[0].Tasks[1].Type);
            Assert.Null(_tickets[0].Tasks[1].MinValue);
            Assert.Null(_tickets[0].Tasks[1].MaxValue);
            Assert.Null(_tickets[0].Tasks[1].DecimalPlaces);
            Assert.Null(_tickets[0].Tasks[1].Unit);

            Assert.Equal(string.Empty,                _tickets[0].Cause);
            Assert.Equal(string.Empty,                _tickets[0].Solution);
            Assert.Equal(string.Empty,                _tickets[0].ExternalId);
            Assert.Equal(string.Empty,                _tickets[0].ExternalMetadata);
            Assert.Equal(string.Empty,                _tickets[0].ExternalStatus);
            Assert.Equal(string.Empty,                _tickets[0].InsightName);
            Assert.Equal(string.Empty,                _tickets[0].Notes);
        }

        [Fact]
        [Trait("Category", "FrequencyUnit")]
        public async Task TicketTemplateService_PerformScheduleHit_exists()
        {
            var hitDate = DateTime.Parse("2021-03-03T00:00:00");
            var occurrence = hitDate.Daydex();

            foreach (var asset in _template1.Assets)
            {
                _workflowService.Setup(wf => wf.TicketOccurrenceExists(templateId1, asset.AssetId, occurrence)).ReturnsAsync(true);
                _workflowService.Setup(wf => wf.TicketOccurrenceExists(templateId1, asset.AssetId.ToString(), occurrence)).ReturnsAsync(true);
            }

            _digitalTwinApi.Setup(x => x.GetTwinIdsByUniqueIdsAsync(It.IsAny<Guid>(), It.IsAny<IEnumerable<Guid>>()))
                .ReturnsAsync((Guid sId, IEnumerable<Guid> aIds) => aIds.Select(x => new TwinIdDto { Id = x.ToString(), UniqueId = x.ToString() }).ToList());

            await _svc.PerformScheduleHit(new ScheduleHit
            {
                ScheduleId = Guid.NewGuid(),
                OwnerId    = templateId1,
                HitDate    = DateTime.Parse("2021-03-03T00:00:00"),
                EventName  = "Test",
            }, "en");

            _workflowService.Verify( wf=> wf.CreateTicket(It.IsAny<List<Ticket>>(), It.IsAny<Guid>(), "en"), Times.Never);

            Assert.Empty(_templates);
        }

        [Trait("Category", "FrequencyUnit")]
        [Fact]
        public async Task TicketTemplateService_PerformScheduleHit_exists2()
        {
            await PerformScheduleHit_Exists("2021-06-15T14:22:00", "2021-06-16T00:00:00", 4185, templateId4, _template4.Assets);
        }

        [Trait("Category", "FrequencyUnit")]
        [Fact]
        public async Task TicketTemplateService_PerformScheduleHit_exists3()
        {
            await PerformScheduleHit_Exists("2021-06-18T17:13:00", "2021-06-20T00:00:00", 4189, templateId5, _template5.Assets);
        }

        private async Task PerformScheduleHit_Exists(string hitDateStr, string hitDate2Str, int occurrence, Guid templateId, IList<TicketAsset> assets)
        {
            var hitDate = DateTime.Parse(hitDateStr); 
            
            foreach(var asset in assets)
                _workflowService.Setup( wf=> wf.TicketOccurrenceExists(templateId, asset.AssetId, occurrence)).ReturnsAsync(false);

            _digitalTwinApi.Setup(x => x.GetTwinIdsByUniqueIdsAsync(It.IsAny<Guid>(), It.IsAny<IEnumerable<Guid>>()))
                .ReturnsAsync((Guid sId, IEnumerable<Guid> aIds) => aIds.Select(x => new TwinIdDto { Id = x.ToString(), UniqueId = x.ToString() }).ToList());

            await _svc.PerformScheduleHit(new ScheduleHit
            {
                ScheduleId = Guid.NewGuid(),
                OwnerId    = templateId,
                HitDate    = hitDate,
                EventName  = "Test",
            }, "en");

            _workflowService.Verify( wf=> wf.CreateTicket(It.IsAny<List<Ticket>>(), It.IsAny<Guid>(), "en"), Times.Once);

            var hitDate2 = DateTime.Parse(hitDate2Str); 

            foreach(var asset in assets)
                _workflowService.Setup( wf=> wf.TicketOccurrenceExists(templateId, asset.AssetId, occurrence)).ReturnsAsync(true);

            foreach (var asset in assets)
            {
                _workflowService.Setup(wf => wf.TicketOccurrenceExists(templateId, asset.AssetId, occurrence)).ReturnsAsync(true);
                _workflowService.Setup(wf => wf.TicketOccurrenceExists(templateId, asset.AssetId.ToString(), occurrence)).ReturnsAsync(true);
            }

            await _svc.PerformScheduleHit(new ScheduleHit
            {
                ScheduleId = Guid.NewGuid(),
                OwnerId    = templateId,
                HitDate    = hitDate2,
                EventName  = "Test",
            }, "en");

            _workflowService.Verify( wf=> wf.CreateTicket(It.IsAny<List<Ticket>>(), It.IsAny<Guid>(), "en"), Times.Once);
        }

        #region CheckSchedule 

        [Trait("Category", "FrequencyUnit")]
        [Fact]
        public void TicketTemplateService_CheckSchedule_NotPending()
        {
            _datetimeService.Setup(w => w.UtcNow).Returns(DateTime.UtcNow);

            var templateId = Guid.NewGuid();

            var (isPending, scheduleHit) = InvokeInstancePrivateMethod<(bool, ScheduleHit)>(_svc, "CheckSchedule", new object[] { templateId, EventDto.MapToModel(_event1), null});

            Assert.False(isPending);
            Assert.True(scheduleHit.OwnerId == templateId);
            Assert.True(scheduleHit.HitDate == EventDto.MapToModel(_event1).StartDate.Date.AddDays(-1));
        }

        [Trait("Category", "FrequencyUnit")]
        [Fact]
        public void TicketTemplateService_CheckSchedule_IsPending()
        {
            var templateId = Guid.NewGuid();

            var (isPending, scheduleHit) = InvokeInstancePrivateMethod<(bool, ScheduleHit)>(_svc, "CheckSchedule", new object[] { templateId, EventDto.MapToModel(_event1), DateTime.Parse("2021-01-14T23:00:00") });

            Assert.True(isPending);
            Assert.True(scheduleHit.OwnerId == templateId);
            Assert.True(scheduleHit.HitDate == EventDto.MapToModel(_event1).StartDate.Date.AddDays(-1));
        }

        #endregion

        #region GetScheduledAssets

        [Trait("Category", "FrequencyUnit")]
        [Theory]
        [InlineData("2021-03-03T00:00:00", 1, 1, 2, false)]
        [InlineData("2021-06-15T14:22:00", 1, 4, 1, false)]
        [InlineData("2021-06-18T17:13:00", 1, 5, 1, false)]
        [InlineData("2021-03-03T00:00:00", 1, 1, 0, true)]
        [InlineData("2021-06-15T14:22:00", 1, 4, 0, true)]
        [InlineData("2021-06-18T17:13:00", 1, 5, 0, true)]
        public async Task TicketTemplateService_GetScheduledAssets_success(string hitDateStr, int advance, int whichTemplate, int expectedCount, bool occurs)
        {
           var hitDate    = DateTime.Parse(hitDateStr); 
           var templateId = _templateIds[whichTemplate-1];
           var template = _sampleTemplates[whichTemplate-1];
           var occurrence = hitDate.AddDays(-advance).Daydex();

            foreach(var asset in template.Assets)
                _workflowService.Setup( wf=> wf.TicketOccurrenceExists(templateId, asset.AssetId, occurrence)).ReturnsAsync(occurs);

           var result = await _svc.GetScheduledAssets(new ScheduleHit            
           {
                ScheduleId = Guid.NewGuid(),
                OwnerId    = templateId,
                HitDate    = hitDate,
                EventName  = "Test",
            });

            Assert.Equal(expectedCount, result.Count);
         }

        #endregion

        #region CreateScheduledTicketForAsset

        [Trait("Category", "FrequencyUnit")]
        [Fact]
        public async Task TicketTemplateService_CreateScheduledTicketForAsset_success()
        {
            var hitDate = DateTime.Parse("2021-03-03T00:00:00");
            var assets = await _svc.GetScheduledAssets(new ScheduleHit
            {
                OwnerId = templateId1,
                HitDate = hitDate,
                EventName = "bob",
                ScheduleId = Guid.NewGuid()
            });

            Assert.Equal(2, assets.Count);

            foreach(var asset in assets)
            {
                _digitalTwinApi.Setup(x => x.GetTwinIdsByUniqueIdsAsync(siteId, new List<Guid> { asset.AssetId }))
                    .ReturnsAsync(new List<TwinIdDto> { new TwinIdDto { Id = "test", UniqueId = asset.AssetId.ToString()} });

                await _svc.CreateScheduledTicketForAsset(asset);
            }

            _workflowService.Verify( s=> s.CreateTicket(It.IsAny<List<Ticket>>(), It.IsAny<Guid>(), It.IsAny<string>()), Times.Exactly(2));
        }

        #endregion

        // https://newbedev.com/net-core-library-how-to-test-private-methods-using-xunit
        public static T InvokeInstancePrivateMethod<T>(object instance, string methodName, object[] callParams)
        {
            var classType = instance.GetType();
            var methodList = classType
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance);

            if (methodList is null || !methodList.Any())
                throw new EntryPointNotFoundException();

            var method = methodList.First(x => x.Name == methodName && !x.IsPublic && x.GetParameters().Length == callParams.Length);

            var output = (T)method.Invoke(instance, callParams);

            return output;
        }

        #region Sample Events

        private static EventDto _event1 = new EventDto
        {
            StartDate      = "2021-01-14T00:00:00",
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

        private static EventDto _event3 = new EventDto
        {
            StartDate = "2021-04-20T00:00:00",
            Occurs    = Event.Recurrence.Monthly,
            Timezone  = "AUS Eastern Standard Time",
            Days      = new List<int> { 20 }
        };

        private static EventDto _event4 = new EventDto
        {
            StartDate      = "2021-06-16T00:00:00",
            Occurs         = Event.Recurrence.Monthly,
            Timezone       = "AUS Eastern Standard Time",
            Days           = new List<int> { 16 }
        };

        private static EventDto _event5 = new EventDto
        {
            StartDate      = "2021-06-20T00:00:00",
            Occurs         = Event.Recurrence.Monthly,
            Interval       = 6,
            Timezone       = "AUS Eastern Standard Time",
            Days           = new List<int> { 20 }
        };

        #endregion
    }
}
