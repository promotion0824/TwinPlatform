using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Willow.Calendar;
using Willow.ExceptionHandling.Exceptions;

namespace Willow.Scheduler.Service.UnitTests
{
    public class SchedulerServiceTests
    {
        private readonly Mock<ISchedulerRepository> _repo;
        private readonly Mock<IScheduleRecipient> _recipient;
        private readonly Mock<ILogger<SchedulerService>> _logger;

        private readonly SchedulerService _schedulerService;

        public SchedulerServiceTests()
        {
            _repo   = new Mock<ISchedulerRepository>();
            _logger = new Mock<ILogger<SchedulerService>>();
            _recipient = new Mock<IScheduleRecipient>();
           
            _schedulerService = new SchedulerService(_repo.Object, _logger.Object, new Dictionary<string, IScheduleRecipient> { {"WorkflowCore:TicketTemplate", _recipient.Object } }, 7);
        }

        #region GetMatching

        [Fact]
        public async Task SchedulerService_GetMatching_empty()
        {
            _repo.Setup( r=> r.GetSchedules() ).ReturnsAsync(new List<Schedule>());

            var result = await _schedulerService.GetMatching(new DateTime(2021, 3, 3, 8, 0, 0, DateTimeKind.Utc)).ToList();

            Assert.Empty(result);
        }

        [Fact]
        public async Task SchedulerService_GetMatching_one_match()
        {
            _repo.Setup( r=> r.GetSchedules() ).ReturnsAsync(new List<Schedule>() 
            {
                new Schedule
                {
                    Recurrence      = JsonConvert.SerializeObject(_event1),
                    RecipientClient = "WorkflowCore",
                    Recipient       = "TicketTemplate"
                }
            });

            var result = await _schedulerService.GetMatching(new DateTime(2021, 3, 3, 8, 0, 0, DateTimeKind.Utc)).ToList();

            Assert.Single(result);
        }

        [Fact]
        public async Task SchedulerService_GetMatching_one_match_today()
        {
            _repo.Setup( r=> r.GetSchedules() ).ReturnsAsync(new List<Schedule>() 
            {
                new Schedule
                {
                    Recurrence      = JsonConvert.SerializeObject(_event5),
                    RecipientClient = "WorkflowCore",
                    Recipient       = "TicketTemplate"
                }
            });

            var result = await _schedulerService.GetMatching(new DateTime(2021, 1, 14, 8, 0, 0, DateTimeKind.Utc)).ToList();

            Assert.Single(result);
        }

        [Fact]
        public async Task SchedulerService_GetMatching_one_nomatches()
        {
            _repo.Setup( r=> r.GetSchedules() ).ReturnsAsync(new List<Schedule>() 
            {
                new Schedule
                {
                    Recurrence      = JsonConvert.SerializeObject(_event2),
                    RecipientClient = "WorkflowCore",
                    Recipient       = "TicketTemplate"
                }
            });

            var result = await _schedulerService.GetMatching(new DateTime(2021, 3, 3, 8, 0, 0, DateTimeKind.Utc).AddHours(-8)).ToList();

            Assert.Empty(result);
        }
                
        [Fact]
        public async Task SchedulerService_GetMatching_3_2matches()
        {
            _repo.Setup( r=> r.GetSchedules() ).ReturnsAsync(new List<Schedule>() 
            {
                new Schedule
                {
                    Recurrence      = JsonConvert.SerializeObject(_event1),
                    RecipientClient = "WorkflowCore",
                    Recipient       = "TicketTemplate"
                },
                new Schedule
                {
                    Recurrence      = JsonConvert.SerializeObject(_event2),
                    RecipientClient = "WorkflowCore",
                    Recipient       = "TicketTemplate"
                },
                new Schedule
                {
                    Recurrence      = JsonConvert.SerializeObject(_event3),
                    RecipientClient = "WorkflowCore",
                    Recipient       = "TicketTemplate"
                }
            });

            var result = await _schedulerService.GetMatching(new DateTime(2021, 3, 3, 8, 0, 0, DateTimeKind.Utc)).ToList();

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task SchedulerService_GetMatching_Monthly_Interval2_OneMonthLater_ShouldNotMatch()
        {
            var yearlyEvent = new Event
            {
                Name = "YearlyEvent",
                StartDate = DateTime.Parse("2023-07-20T00:00:00"),
                Occurs = Event.Recurrence.Monthly,
                Timezone = "Eastern Standard Time",
                Interval =2,
                Days = new List<int> { 20 }
            };

            var oneMonthLater = yearlyEvent.StartDate.ToUtc(yearlyEvent.Timezone).AddMonths(1);

            _repo.Setup(r => r.GetSchedules()).ReturnsAsync(new List<Schedule>()
            {
                new Schedule
                {
                    Recurrence      = JsonConvert.SerializeObject(yearlyEvent),
                    RecipientClient = "WorkflowCore",
                    Recipient       = "TicketTemplate"
                }
            });

            var result = await _schedulerService.GetMatching(oneMonthLater).ToList();

            Assert.Empty(result);
        }

        [Fact]
        public async Task SchedulerService_GetMatching_Monthly_Interval2_TwoMonthLater_ShouldMatch()
        {
            var yearlyEvent = new Event
            {
                Name = "YearlyEvent",
                StartDate = DateTime.Parse("2023-07-20T00:00:00"),
                Occurs = Event.Recurrence.Monthly,
                Timezone = "Eastern Standard Time",
                Interval = 2,
                Days = new List<int> { 20 }
            };

            var oneMonthLater = yearlyEvent.StartDate.ToUtc(yearlyEvent.Timezone).AddMonths(2);

            _repo.Setup(r => r.GetSchedules()).ReturnsAsync(new List<Schedule>()
            {
                new Schedule
                {
                    Recurrence      = JsonConvert.SerializeObject(yearlyEvent),
                    RecipientClient = "WorkflowCore",
                    Recipient       = "TicketTemplate"
                }
            });

            var result = await _schedulerService.GetMatching(oneMonthLater).ToList();

            Assert.Equal(1, result.Count);
        }

        [Fact]
        public async Task SchedulerService_GetMatching_Monthly_Interval1_InitialDate_ShouldMatch()
        {
            var yearlyEvent = new Event
            {
                Name = "YearlyEvent",
                StartDate = DateTime.Parse("2023-07-20T00:00:00"),
                Occurs = Event.Recurrence.Yearly,
                Timezone = "Eastern Standard Time",
                Interval = 1,
                Days = new List<int> { 20 }
            };

            var initialDate = yearlyEvent.StartDate.ToUtc(yearlyEvent.Timezone);

            _repo.Setup(r => r.GetSchedules()).ReturnsAsync(new List<Schedule>()
            {
                new Schedule
                {
                    Recurrence      = JsonConvert.SerializeObject(yearlyEvent),
                    RecipientClient = "WorkflowCore",
                    Recipient       = "TicketTemplate"
                }
            });

            var result = await _schedulerService.GetMatching(initialDate).ToList();

            Assert.Equal(1, result.Count);
        }

        [Fact]
        public async Task SchedulerService_GetMatching_Yearly_ExactStartDatePlusOneMonth_ShouldNotMatch()
        {
            var yearlyEvent = new Event
            {
                Name = "YearlyEvent",
                StartDate = DateTime.Parse("2023-07-20T00:00:00"),
                Occurs = Event.Recurrence.Yearly,
                Timezone = "Eastern Standard Time",
                Interval = 5,
                Days = new List<int> { 20 }
            };

            var oneMonthLater = yearlyEvent.StartDate.ToUtc(yearlyEvent.Timezone).AddMonths(1);

            _repo.Setup(r => r.GetSchedules()).ReturnsAsync(new List<Schedule>()
            {
                new Schedule
                {
                    Recurrence      = JsonConvert.SerializeObject(yearlyEvent),
                    RecipientClient = "WorkflowCore",
                    Recipient       = "TicketTemplate"
                }
            });

            var result = await _schedulerService.GetMatching(oneMonthLater).ToList();

            Assert.Empty(result);
        }

        [Fact]
        public async Task SchedulerService_GetMatching_Yearly_ExactStartDatePlusOneIntervalLessThanAMonth_ShouldNotMatch()
        {
            var yearlyEvent = new Event
            {
                Name = "YearlyEvent",
                StartDate = DateTime.Parse("2023-07-20T00:00:00"),
                Occurs = Event.Recurrence.Yearly,
                Timezone = "Eastern Standard Time",
                Interval = 1,
                Days = new List<int> { 20 }
            };

            var oneMonthLater = yearlyEvent.StartDate.ToUtc(yearlyEvent.Timezone).AddYears(1).AddMonths(-1);

            _repo.Setup(r => r.GetSchedules()).ReturnsAsync(new List<Schedule>()
            {
                new Schedule
                {
                    Recurrence      = JsonConvert.SerializeObject(yearlyEvent),
                    RecipientClient = "WorkflowCore",
                    Recipient       = "TicketTemplate"
                }
            });

            var result = await _schedulerService.GetMatching(oneMonthLater).ToList();

            Assert.Empty(result);
        }
        #endregion

        #region GetMatchingByOwnerIds

        [Fact]
        public async Task SchedulerService_GetMatchingByOwnerIds_empty()
        {
            var queryList = new List<Guid> { Guid.NewGuid() };

            _repo.Setup( r=> r.GetSchedulesByOwnerId(queryList) ).ReturnsAsync(new List<Schedule>());

            var result = (await _schedulerService.GetMatchingByOwnerIds(new List<Guid> { Guid.NewGuid() }, new DateTime(2021, 3, 3, 8, 0, 0, DateTimeKind.Utc))).ToList();

            Assert.Empty(result);
        }

        [Fact]
        public async Task SchedulerService_GetMatchingByOwnerIds_nomatching()
        {
            var repoList  = new List<Guid> { Guid.NewGuid() };
            var queryList = new List<Guid> { Guid.NewGuid() };

            _repo.Setup( r=> r.GetSchedulesByOwnerId(repoList) ).ReturnsAsync(new List<Schedule>( new List<Schedule> { new Schedule { OwnerId = Guid.NewGuid() }}));

            var result = (await _schedulerService.GetMatchingByOwnerIds(queryList, new DateTime(2021, 3, 3, 8, 0, 0, DateTimeKind.Utc))).ToList();

            Assert.Empty(result);
        }

        #endregion

        #region GetSchedulesByOwnerId

        [Fact]
        public async Task SchedulerService_GetSchedulesByOwnerId_one()
        {
            var ownerId = Guid.NewGuid();

            var repoList  = new List<Guid> { ownerId };
            var queryList = new List<Guid> { ownerId };

            _repo.Setup( r=> r.GetSchedulesByOwnerId(repoList) ).ReturnsAsync(new List<Schedule>( new List<Schedule> 
                { 
                    new Schedule { OwnerId = ownerId, Id = Guid.NewGuid(), Active = true, Recurrence = JsonConvert.SerializeObject(_event1) }
                }));

            var result = (await _schedulerService.GetSchedulesByOwnerId(new DateTime(2021, 2, 24, 8, 0, 0, DateTimeKind.Utc), queryList));

            Assert.Single(result);

            Assert.Equal("Event1", result[0].EventName);
        }

        #endregion

        #region CheckSchedules

        [Fact]
        public async Task SchedulerService_CheckSchedules_one_match()
        {
            _repo.Setup( r=> r.GetSchedules() ).ReturnsAsync(new List<Schedule>() 
            {
                new Schedule
                {
                    Recurrence      = JsonConvert.SerializeObject(_event1),
                    RecipientClient = "WorkflowCore",
                    Recipient       = "TicketTemplate"
                }
            });

            await _schedulerService.CheckSchedules(new DateTime(2021, 3, 3, 8, 0, 0, DateTimeKind.Utc).AddDays(-7), "en");

            _logger.Verify( log=> log.Log(LogLevel.Error,
                                          It.IsAny<EventId>(),
                                          It.IsAny<It.IsAnyType>(),
                                          It.IsAny<Exception>(),
                                          (Func<It.IsAnyType, Exception, string>) It.IsAny<object>()), Times.Never);
            _logger.Verify( log=> log.Log(LogLevel.Information,
                                          It.IsAny<EventId>(),
                                          It.IsAny<It.IsAnyType>(),
                                          It.IsAny<Exception>(),
                                          (Func<It.IsAnyType, Exception, string>) It.IsAny<object>()), Times.Exactly(2));

            _recipient.Verify( r=> r.PerformScheduleHit(It.IsAny<ScheduleHit>(), "en"), Times.Once);
        }

        [Fact]
        public async Task SchedulerService_CheckSchedules_one_match2()
        {
            _repo.Setup( r=> r.GetSchedules() ).ReturnsAsync(new List<Schedule>() 
            {
                new Schedule
                {
                    Recurrence      = JsonConvert.SerializeObject(_event4),
                    RecipientClient = "WorkflowCore",
                    Recipient       = "TicketTemplate"
                }
            });

            var utcNow = new DateTime(2021, 4, 12, 19, 35, 0, DateTimeKind.Utc);

            await _schedulerService.CheckSchedules(utcNow, "en");

            _logger.Verify( log=> log.Log(LogLevel.Error,
                                          It.IsAny<EventId>(),
                                          It.IsAny<It.IsAnyType>(),
                                          It.IsAny<Exception>(),
                                          (Func<It.IsAnyType, Exception, string>) It.IsAny<object>()), Times.Never);
            _logger.Verify( log=> log.Log(LogLevel.Information,
                                          It.IsAny<EventId>(),
                                          It.IsAny<It.IsAnyType>(),
                                          It.IsAny<Exception>(),
                                          (Func<It.IsAnyType, Exception, string>) It.IsAny<object>()), Times.Exactly(2));

            _recipient.Verify( r=> r.PerformScheduleHit(It.IsAny<ScheduleHit>(), "en"), Times.Once);
        }


        [Fact]
        public async Task SchedulerService_CheckSchedules_notemplate()
        {
            _repo.Setup( r=> r.GetSchedules() ).ReturnsAsync(new List<Schedule>() 
            {
                new Schedule
                {
                    Recurrence      = JsonConvert.SerializeObject(_event4),
                    RecipientClient = "WorkflowCore",
                    Recipient       = "TicketTemplate"
                }
            });

            _recipient.Setup( r=> r.PerformScheduleHit(It.IsAny<ScheduleHit>(), "en")).Throws(new NotFoundException("TicketTemplate not found"));

            var utcNow = new DateTime(2021, 4, 12, 19, 35, 0, DateTimeKind.Utc);

            await _schedulerService.CheckSchedules(utcNow, "en");

            _logger.Verify( log=> log.Log(LogLevel.Information,
                                          It.IsAny<EventId>(),
                                          It.IsAny<It.IsAnyType>(),
                                          It.IsAny<Exception>(),
                                          (Func<It.IsAnyType, Exception, string>) It.IsAny<object>()), Times.Exactly(2));

            _recipient.Verify( r=> r.PerformScheduleHit(It.IsAny<ScheduleHit>(), "en"), Times.Once);
            _repo.Verify( r=> r.DeleteSchedule(It.IsAny<Guid>()), Times.Once);
        }

        #endregion
        
        #region Sample Events

        private static Event _event1 = new Event
        {
            Name           = "Event1",
            StartDate      = DateTime.Parse("2021-01-14T00:00:00"),
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

        private static Event _event2 = new Event
        {
            Name           = "Event2",
            StartDate      = DateTime.Parse("2021-01-14T00:00:00"),
            Occurs         = Event.Recurrence.Monthly,
            Timezone       = "Pacific Standard Time",
            DayOccurrences = new List<Event.DayOccurrence>
            {
                new Event.DayOccurrence
                {
                    Ordinal = 1,
                    DayOfWeek = DayOfWeek.Friday
                }
            }
        };

       private static Event _event3 = new Event
        {
            Name           = "Event3",
            StartDate      = DateTime.Parse("2021-01-14T00:00:00"),
            Timezone       = "Pacific Standard Time",
            Occurs         = Event.Recurrence.Monthly,
            Days           = new List<int>
            {
                3, 7
            }
        };

        
        private static Event _event4 = new Event
        {
            Name      = "Event4",
            StartDate = new DateTime(2021, 4, 20, 2, 0, 0, DateTimeKind.Unspecified),
            Occurs    = Event.Recurrence.Monthly,
            Timezone  = "AUS Eastern Standard Time",
            Days      = new List<int> { 20 }
        };

        private static Event _event5 = new Event
        {
            Name           = "Event5",
            StartDate      = DateTime.Parse("2021-01-14T00:00:00"),
            Occurs         = Event.Recurrence.Monthly,
            Timezone       = "Pacific Standard Time",
             Days          = new List<int> { 14 }
        };

        #endregion

       
    }
}
