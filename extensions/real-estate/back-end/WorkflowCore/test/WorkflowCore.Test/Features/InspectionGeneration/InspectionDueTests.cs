using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using WorkflowCore.Models;
using Xunit;

namespace WorkflowCore.Test.Features.InspectionGeneration
{
	public class InspectionDueTests
	{
		[Theory]
		[InlineData(1, SchedulingUnit.Hours, "2020-07-01T15:00:00", "2020-07-01T14:00:00", true)]
		[InlineData(2, SchedulingUnit.Hours, "2020-07-01T15:00:00", "2020-07-01T14:00:00", false)]
		[InlineData(4, SchedulingUnit.Days, "2020-07-16T15:00:00", "2020-06-30T15:00:00", true)]
		[InlineData(4, SchedulingUnit.Days, "2020-07-16T15:00:00", "2020-06-29T15:00:00", false)]
		[InlineData(1, SchedulingUnit.Weeks, "2020-07-06T15:00:00", "2020-06-29T15:00:00", true)]
		[InlineData(2, SchedulingUnit.Weeks, "2020-07-13T15:00:00", "2020-06-29T15:00:00", true)]
		[InlineData(2, SchedulingUnit.Weeks, "2020-07-07T15:00:00", "2020-06-29T15:00:00", false)]
		[InlineData(3, SchedulingUnit.Months, "2020-10-07T15:00:00", "2020-07-07T15:00:00", true)]
		[InlineData(2, SchedulingUnit.Months, "2020-10-07T15:00:00", "2020-07-07T15:00:00", false)]
		[InlineData(1, SchedulingUnit.Months, "2020-11-30T15:00:00", "2020-07-31T15:00:00", true)]
		[InlineData(1, SchedulingUnit.Months, "2020-02-29T15:00:00", "2020-01-31T15:00:00", true)]
		[InlineData(1, SchedulingUnit.Years, "2021-10-07T15:00:00", "2017-10-07T15:00:00", true)]
		[InlineData(2, SchedulingUnit.Years, "2022-10-07T15:00:00", "2017-10-07T15:00:00", false)]
		[InlineData(1, SchedulingUnit.Years, "2020-02-29T15:00:00", "2019-02-28T15:00:00", false)]
		[InlineData(1, SchedulingUnit.Years, "2021-02-28T15:00:00", "2020-02-29T15:00:00", true)]
		[InlineData(3, SchedulingUnit.Months, "2024-02-28T15:00:00", "2020-02-29T15:00:00", false)]
		[InlineData(3, SchedulingUnit.Months, "2024-02-29T15:00:00", "2020-02-29T15:00:00", true)]
		[InlineData(1, SchedulingUnit.Months, "2024-02-29T15:00:00", "2021-02-28T15:00:00", false)]
        [InlineData(2, SchedulingUnit.Weeks, "2020-07-07T15:00:00", "2020-06-29T15:00:00", false, "Monday,Friday")]
        [InlineData(2, SchedulingUnit.Weeks, "2020-07-07T15:00:00", "2020-07-02T15:00:00", true, "Tuesday")]
        [InlineData(2, SchedulingUnit.Weeks, "2020-07-07T15:00:00", "2020-06-23T15:00:00", true, "Tuesday")]

public void InspectionExtensions_IsDue_NoRecordsExisting(int frequency,
																 SchedulingUnit frequencyUnit,
																 string now,
																 string start,
																 bool isDue,
                                                                 string daysOfWeeks=null) {
			var nowDateTime = DateTime.Parse(now);
			var startDateTime = DateTime.Parse(start);

			var instpection = new Inspection()
			{
				Frequency = frequency,
				FrequencyUnit = frequencyUnit,
				StartDate = startDateTime,
                FrequencyDaysOfWeek = string.IsNullOrEmpty(daysOfWeeks)?null:daysOfWeeks.Split(',').Select(c=>Enum.Parse<DayOfWeek>(c))
			};

			instpection.IsDue(nowDateTime, TimeZoneInfo.Utc.Id).Should().Be(isDue);
		}



		[Theory]
		[InlineData(1, SchedulingUnit.Hours, "2020-07-01T15:00:00", "2020-07-01T14:00:00", "2020-07-01T14:00:00", true)]
		[InlineData(2, SchedulingUnit.Hours, "2020-07-01T15:00:00", "2020-07-01T14:00:00", "2020-07-01T14:00:00", false)]
		[InlineData(4, SchedulingUnit.Days, "2020-07-16T15:00:00", "2020-06-30T15:00:00", "2020-06-30T15:00:00", true)]
		[InlineData(4, SchedulingUnit.Days, "2020-07-16T15:00:00", "2020-06-29T15:00:00", "2020-06-29T15:00:00", true)]
		[InlineData(1, SchedulingUnit.Weeks, "2020-07-06T15:00:00", "2020-06-29T15:00:00", "2020-06-29T15:00:00", true)]
		[InlineData(2, SchedulingUnit.Weeks, "2020-07-13T15:00:00", "2020-06-29T15:00:00", "2020-06-29T15:00:00", true)]
		[InlineData(2, SchedulingUnit.Weeks, "2020-07-07T15:00:00", "2020-06-29T15:00:00", "2020-06-29T15:00:00", false)]
		[InlineData(3, SchedulingUnit.Months, "2020-10-07T15:00:00", "2020-07-07T15:00:00", "2020-07-07T15:00:00", true)]
		[InlineData(2, SchedulingUnit.Months, "2020-10-07T15:00:00", "2020-07-07T15:00:00", "2020-07-07T15:00:00", false)]
		[InlineData(1, SchedulingUnit.Months, "2020-11-30T15:00:00", "2020-07-31T15:00:00", "2020-07-31T15:00:00", true)]
		[InlineData(1, SchedulingUnit.Months, "2020-02-29T15:00:00", "2020-01-31T15:00:00", "2020-01-31T15:00:00", true)]
		[InlineData(1, SchedulingUnit.Years, "2021-10-07T15:00:00", "2017-10-07T15:00:00", "2017-10-07T15:00:00", true)]
		[InlineData(2, SchedulingUnit.Years, "2022-10-07T15:00:00", "2017-10-07T15:00:00", "2017-10-07T15:00:00", false)]
		[InlineData(1, SchedulingUnit.Years, "2020-02-29T15:00:00", "2019-02-28T15:00:00", "2019-02-28T15:00:00", false)]
		[InlineData(1, SchedulingUnit.Years, "2021-02-28T15:00:00", "2020-02-29T15:00:00", "2020-02-29T15:00:00", true)]
        [InlineData(1, SchedulingUnit.Weeks, "2020-07-10T15:00:00", "2020-06-29T15:00:00", "2020-07-06T15:00:00", true, "Monday,Friday")]
        [InlineData(2, SchedulingUnit.Weeks, "2020-07-13T15:00:00", "2020-06-29T15:00:00", "2020-06-29T15:00:00", true, "Monday,Friday")]
        [InlineData(2, SchedulingUnit.Weeks, "2020-07-07T15:00:00", "2020-06-29T15:00:00", "2020-06-29T15:00:00", false, "Monday,Friday")]
public void InspectionExtensions_IsDue_PreExisistingRecords(int frequency,
																 SchedulingUnit frequencyUnit,
																 string now,
																 string start,
																 string lastRecordEffictive,
																 bool isDue,
                                                                 string daysOfWeeks = null)
		{
			var nowDateTime = DateTime.Parse(now);
			var startDateTime = DateTime.Parse(start);
			var lastRecordEffictiveDateTime = DateTime.Parse(lastRecordEffictive);

			var instpection = new Inspection()
			{
				Frequency = frequency,
				FrequencyUnit = frequencyUnit,
				StartDate = startDateTime,
				LastRecord = new InspectionRecord()
				{
					EffectiveDate = lastRecordEffictiveDateTime
				},

                FrequencyDaysOfWeek = string.IsNullOrEmpty(daysOfWeeks) ? null : daysOfWeeks.Split(',').Select(c => Enum.Parse<DayOfWeek>(c))
            };

            instpection.IsDue(nowDateTime, TimeZoneInfo.Utc.Id).Should().Be(isDue);
		}
	}
}
