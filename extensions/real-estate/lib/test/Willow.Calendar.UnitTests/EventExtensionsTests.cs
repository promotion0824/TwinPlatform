using System;
using System.Collections.Generic;
using Xunit;

using Willow.Calendar;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Willow.Calendar.UnitTests
{
    public class EventExtensionsTests
    {
        #region Monthly

        [Fact]
        public void Event_Matches_Monthly()
        {
            Assert.False(_event1.Matches(DateTime.Parse("2021-01-13T00:00:00")));
            Assert.False(_event1.Matches(DateTime.Parse("2021-01-19T00:00:00")));
            Assert.False(_event1.Matches(DateTime.Parse("2021-01-24T00:00:00")));
            Assert.False(_event1.Matches(DateTime.Parse("2021-03-02T00:00:00")));
            Assert.True(_event1.Matches(DateTime.Parse("2021-03-03T00:00:00")));
            Assert.True(_event1.Matches(DateTime.Parse("2023-04-05T00:00:00")));
            Assert.True(_event1.Matches(DateTime.Parse("2022-11-02T00:00:00")));
        }

        [Fact]
        public void Event_Matches_Monthly2()
        {
			Assert.False(_event2.Matches(DateTime.Parse("2021-01-13T00:00:00")));
			Assert.False(_event2.Matches(DateTime.Parse("2021-01-19T00:00:00")));
			Assert.False(_event2.Matches(DateTime.Parse("2021-01-24T00:00:00")));
			Assert.False(_event2.Matches(DateTime.Parse("2021-03-03T00:00:00")));
			Assert.False(_event2.Matches(DateTime.Parse("2021-03-17T00:00:00")));
			Assert.True(_event2.Matches(DateTime.Parse("2021-03-25T00:00:00")));
			Assert.True(_event2.Matches(DateTime.Parse("2021-04-08T00:00:00")));
			Assert.True(_event2.Matches(DateTime.Parse("2021-04-22T00:00:00")));
		}

        [Fact]
        public void Event_Matches_Monthly3()
        {
            Assert.True(_event10.Matches(DateTime.Parse("2021-04-23T14:00:00")));
        }

        [Fact]
        public void Event_Matches_Monthly_today()
        {
            Assert.True(_event12.Matches(DateTime.Parse("2021-01-07T00:00:00")));
        }

        #endregion

        #region Hourly

        [Fact]
        public void Event_Matches_Hourly_every4hours()
        {
            Assert.False(_event3.Matches(DateTime.Parse("2021-01-13T00:00:00")));
            Assert.False(_event3.Matches(DateTime.Parse("2021-01-19T00:31:00")));
            Assert.False(_event3.Matches(DateTime.Parse("2021-01-24T01:54:00")));

            Assert.False(_event3.Matches(DateTime.Parse("2021-03-04T00:09:00")));
            Assert.False(_event3.Matches(DateTime.Parse("2021-03-04T04:06:12")));
            Assert.False(_event3.Matches(DateTime.Parse("2021-03-04T08:45:00")));
            Assert.False(_event3.Matches(DateTime.Parse("2021-03-04T10:00:00")));

            Assert.True(_event3.Matches(DateTime.Parse("2021-03-03T00:00:00")));
            Assert.True(_event3.Matches(DateTime.Parse("2021-03-03T04:00:00")));
            Assert.True(_event3.Matches(DateTime.Parse("2021-03-03T08:01:00")));
            Assert.True(_event3.Matches(DateTime.Parse("2021-03-03T12:02:00")));
            Assert.True(_event3.Matches(DateTime.Parse("2021-03-03T16:03:00")));
            Assert.True(_event3.Matches(DateTime.Parse("2021-03-03T20:04:00")));

            Assert.True(_event3.Matches(DateTime.Parse("2021-03-04T00:00:00")));
            Assert.True(_event3.Matches(DateTime.Parse("2021-03-04T04:00:00")));
            Assert.True(_event3.Matches(DateTime.Parse("2021-03-04T08:00:00")));
            Assert.True(_event3.Matches(DateTime.Parse("2021-03-04T12:00:00")));
            Assert.True(_event3.Matches(DateTime.Parse("2021-03-04T16:00:00")));
            Assert.True(_event3.Matches(DateTime.Parse("2021-03-04T20:00:00")));
        }

        [Fact]
        public void Event_Matches_Hourly_every1hours()
        {
            Assert.False(_event4.Matches(DateTime.Parse("2021-01-13T00:00:00")));
            Assert.False(_event4.Matches(DateTime.Parse("2021-01-19T00:31:00")));
            Assert.False(_event4.Matches(DateTime.Parse("2021-01-24T01:54:00")));
                               
            Assert.False(_event4.Matches(DateTime.Parse("2021-03-04T00:09:00")));
            Assert.False(_event4.Matches(DateTime.Parse("2021-03-04T04:06:12")));
            Assert.False(_event4.Matches(DateTime.Parse("2021-03-04T08:45:00")));
                              
            Assert.True(_event4.Matches(DateTime.Parse("2021-03-03T00:00:00")));
            Assert.True(_event4.Matches(DateTime.Parse("2021-03-03T01:00:00")));
            Assert.True(_event4.Matches(DateTime.Parse("2021-03-03T02:01:00")));
            Assert.True(_event4.Matches(DateTime.Parse("2021-03-03T03:02:00")));
            Assert.True(_event4.Matches(DateTime.Parse("2021-03-03T10:03:00")));
            Assert.True(_event4.Matches(DateTime.Parse("2021-03-03T12:04:00")));
                               
            Assert.True(_event4.Matches(DateTime.Parse("2021-03-04T13:00:00")));
            Assert.True(_event4.Matches(DateTime.Parse("2021-03-04T14:00:00")));
            Assert.True(_event4.Matches(DateTime.Parse("2021-03-04T15:00:00")));
            Assert.True(_event4.Matches(DateTime.Parse("2021-03-04T16:00:00")));
            Assert.True(_event4.Matches(DateTime.Parse("2021-03-04T17:00:00")));
            Assert.True(_event4.Matches(DateTime.Parse("2021-03-04T18:00:00")));
            Assert.True(_event4.Matches(DateTime.Parse("2021-03-04T00:00:00")));
        }

        #endregion
        
        #region Daily

        [Fact]
        public void Event_Matches_Daily_everyday_at_8am()
        {
            Assert.False(_event5.Matches(DateTime.Parse("2021-01-13T00:00:00")));
            Assert.False(_event5.Matches(DateTime.Parse("2021-01-19T00:31:00")));
            Assert.False(_event5.Matches(DateTime.Parse("2021-01-24T01:54:00")));
            Assert.False(_event5.Matches(DateTime.Parse("2021-03-04T08:06:00")));
            Assert.False(_event5.Matches(DateTime.Parse("2021-03-04T08:09:00")));

            Assert.True(_event5.Matches(DateTime.Parse("2021-03-03T08:00:00")));
            Assert.True(_event5.Matches(DateTime.Parse("2021-03-04T08:00:00")));
            Assert.True(_event5.Matches(DateTime.Parse("2021-03-05T08:01:00")));
            Assert.True(_event5.Matches(DateTime.Parse("2021-03-12T18:02:00")));
            Assert.True(_event5.Matches(DateTime.Parse("2021-03-14T18:03:00")));
            Assert.True(_event5.Matches(DateTime.Parse("2021-03-31T18:04:00")));
        }

		#endregion

		#region Weekly

		[Fact]
		public void Event_Matches_Weekly()
		{
			Assert.True(_event15.Matches(DateTime.Parse("2023-02-09T00:00:00")));
			Assert.True(_event15.Matches(DateTime.Parse("2023-03-02T00:00:00")));
			Assert.True(_event16.Matches(DateTime.Parse("2024-01-18T00:00:00")));

			Assert.False(_event15.Matches(DateTime.Parse("2023-02-16T00:00:00")));
			Assert.False(_event16.Matches(DateTime.Parse("2023-11-01T00:00:00")));
		}

		#endregion

		#region NextOccurrence

		[Fact]
        public void Event_NextOccurrence()
        {
            Assert.Equal(DateTime.Parse("2021-03-03T00:00:00"), _event1.NextOccurrence(DateTime.Parse("2021-02-14T00:00:00")));
            Assert.Equal(DateTime.Parse("2021-04-07T00:00:00"), _event1.NextOccurrence(DateTime.Parse("2021-03-03T00:00:00")));

            Assert.Equal(DateTime.Parse("2021-03-11T00:00:00"), _event2.NextOccurrence(DateTime.Parse("2021-03-01T00:00:00")));
            Assert.Equal(DateTime.Parse("2021-03-25T00:00:00"), _event2.NextOccurrence(DateTime.Parse("2021-03-11T00:00:00")));

            Assert.Equal(DateTime.MaxValue,                     _event6.NextOccurrence(DateTime.Parse("2021-04-21T00:00:00")));
            Assert.Equal(DateTime.Parse("2021-04-07T00:00:00"), _event7.NextOccurrence(DateTime.Parse("2021-03-11T00:00:00")));
            Assert.Equal(DateTime.Parse("2021-04-27T00:00:00"), _event11.NextOccurrence(DateTime.Parse("2021-04-20T00:00:00")));
        }

        [Fact]
        public void Event_NextOccurrence_ordinal()
        {
            Assert.Equal(DateTime.Parse("2021-03-03T00:00:00"), _event1.NextOccurrence(DateTime.Parse("2021-02-14T00:00:00")));
        }

        [Fact]
        public void Event_NextOccurrence_days()
        {
			Assert.Equal(DateTime.Parse("2021-07-12T00:00:00"), _event8.NextOccurrence(DateTime.Parse("2021-04-12T18:20:00")));
			Assert.Equal(DateTime.Parse("2021-10-12T00:00:00"), _event8.NextOccurrence(DateTime.Parse("2021-09-27T18:20:00")));
			Assert.Equal(DateTime.Parse("2022-01-12T00:00:00"), _event8.NextOccurrence(DateTime.Parse("2021-10-21T18:20:00")));
		}

        [Fact]
        public void Event_NextOccurrence_currentmonth()
        {
            Assert.Equal(DateTime.Parse("2021-04-24T00:00:00"), _event9.NextOccurrence(DateTime.Parse("2021-04-13T18:20:00")));
            Assert.Equal(DateTime.Parse("2021-05-24T00:00:00"), _event9.NextOccurrence(DateTime.Parse("2021-04-24T18:20:00")));
        }

        [Fact]
        public void Event_NextOccurrence_currentmonth2()
        {
            Assert.Equal(DateTime.Parse("2021-04-23T00:00:00"), _event10.NextOccurrence(DateTime.Parse("2021-04-16T14:00:00")));
        }
       
        [Fact]
        public void Event_NextOccurrence_today()
        {
            Assert.Equal(DateTime.Parse("2021-09-01T00:00:00"), _event13.NextOccurrence(DateTime.Parse("2021-06-01T14:00:00")));
        }

        [Theory]
        [InlineData("2021-11-30T16:41:35")]
        [InlineData("2021-12-01T16:41:35")]
        [InlineData("2021-12-02T16:41:35")]
        [InlineData("2021-12-03T16:41:35")]
        public void Event_NextOccurrence_eom(string now)
        {
            Assert.Equal(DateTime.Parse("2022-02-28T00:00:00"), _event14.NextOccurrence(DateTime.Parse(now)));
        }

        [Fact]
        public void Event_Serialize()
        {
            var sEvent = JsonConvert.SerializeObject(_event1);

            Assert.NotNull(sEvent);
        }

        #endregion

        #region Sample Events

        // First Wednesday of every month
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

        // 2nd and 4th Thursay of every month
        private static Event _event2 = new Event
        {
            StartDate      = DateTime.Parse("2021-01-14T00:00:00"),
            Occurs         = Event.Recurrence.Monthly,
            DayOccurrences = new List<Event.DayOccurrence>
            {
                new Event.DayOccurrence
                {
                    Ordinal = 2,
                    DayOfWeek = DayOfWeek.Thursday
                },
                new Event.DayOccurrence
                {
                    Ordinal = 4,
                    DayOfWeek = DayOfWeek.Thursday
                }
            }
        };

        // Every 4 hours
        private static Event _event3 = new Event
        {
            StartDate      = DateTime.Parse("2021-01-14T00:00:00"),
            Occurs         = Event.Recurrence.Hourly,
            Interval       = 4
        };

        // Every 1 hour
        private static Event _event4 = new Event
        {
            StartDate      = DateTime.Parse("2021-01-14T00:00:00"),
            Occurs         = Event.Recurrence.Hourly,
            Interval       = 1
        };

        // Every day
        private static Event _event5 = new Event
        {
            StartDate      = DateTime.Parse("2021-01-14T08:00:00"),
            Occurs         = Event.Recurrence.Daily,
            Interval       = 1
        };

        // First Wednesday of every month
        private static Event _event6 = new Event
        {
            StartDate      = DateTime.Parse("2021-01-14T00:00:00"),
            EndDate        = DateTime.Parse("2021-04-14T00:00:00"),
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

        // First Wednesday of every month
        private static Event _event7 = new Event
        {
            StartDate      = DateTime.Parse("2021-07-01T00:00:00"),
            EndDate        = DateTime.Parse("2021-12-14T00:00:00"),
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

        // 12th of month
        private static Event _event8 = new Event
        {
            StartDate      = new DateTime(2021, 04, 12, 0, 0, 0, DateTimeKind.Unspecified),
            EndDate        = new DateTime(2021, 12, 14, 0, 0, 0, DateTimeKind.Unspecified),
            Timezone       = "AUS Eastern Standard Time",
            Occurs         = Event.Recurrence.Monthly,
            Interval       = 3,
            MaxOccurrences = 0,
            Days           = new List<int> { 12 }
        };

		// 24th of month
		private static Event _event9 = new Event
        {
            StartDate      = new DateTime(2021, 01, 24, 0, 0, 0, DateTimeKind.Unspecified),
            EndDate        = new DateTime(2022, 12, 14, 0, 0, 0, DateTimeKind.Unspecified),
            Timezone       = "AUS Eastern Standard Time",
            Occurs         = Event.Recurrence.Monthly,
            Interval       = 1,
            MaxOccurrences = 0,
            Days           = new List<int> { 24 }
        };

        // 24th of month
        private static Event _event10 = new Event
        {
            StartDate      = new DateTime(2021,  4, 23, 0, 0, 0, DateTimeKind.Unspecified),
            EndDate        = new DateTime(2022, 12, 14, 0, 0, 0, DateTimeKind.Unspecified),
            Timezone       = "AUS Eastern Standard Time",
            Occurs         = Event.Recurrence.Monthly,
            Interval       = 3,
            MaxOccurrences = 0,
            Days           = new List<int> { 23 }
        };

        // 12th of month
        private static Event _event11 = new Event
        {
            StartDate      = new DateTime(2021, 03, 27, 0, 0, 0, DateTimeKind.Unspecified),
            Timezone       = "Eastern Standard Time",
            Occurs         = Event.Recurrence.Monthly,
            Interval       = 1,
            MaxOccurrences = 0,
            Days           = new List<int> { 27 }
        };

        private static Event _event12 = new Event
        {
            StartDate      = DateTime.Parse("2021-01-07T00:00:00"),
            Occurs         = Event.Recurrence.Monthly,
            Days           = new List<int> { 7 }
        };

        private static Event _event13 = new Event
        {
            StartDate      = DateTime.Parse("2021-03-01T00:00:00"),
            Occurs         = Event.Recurrence.Monthly,
            Interval       = 3,
            Days           = new List<int> { 1 }
        };        
        
        private static Event _event14 = new Event
        {
            StartDate      = DateTime.Parse("2021-05-30T00:00:00"),
            Occurs         = Event.Recurrence.Monthly,
            Interval       = 3,
            Days           = new List<int> { 30 },
            Timezone       = "Eastern Standard Time"
        };

		// Every 3 weeks
		private static Event _event15 = new Event
		{
			StartDate = new DateTime(2023, 01, 19, 0, 0, 0, DateTimeKind.Unspecified),
			Timezone = "AUS Eastern Standard Time",
			Occurs = Event.Recurrence.Weekly,
			Interval = 3,
		};

		// Every 52 weeks
		private static Event _event16 = new Event
		{
			StartDate = new DateTime(2023, 01, 19, 0, 0, 0, DateTimeKind.Unspecified),
			Timezone = "AUS Eastern Standard Time",
			Occurs = Event.Recurrence.Weekly,
			Interval = 52
		};

		#endregion
	}
}
