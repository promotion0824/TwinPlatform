namespace Willow.LiveData.Core.Tests.UnitTests
{
    using System;
    using FluentAssertions;
    using NUnit.Framework;
    using Willow.Infrastructure.Exceptions;
    using Willow.LiveData.Core.Common;

    public class DateTimeIntervalServiceTests
    {
        private readonly DateTimeIntervalService dateTimeIntervalService;
        private readonly DateTime startDateTime = new(2000, 1, 1, 0, 0, 0);

        public DateTimeIntervalServiceTests()
        {
            dateTimeIntervalService = new DateTimeIntervalService();
        }

        [TestCase("1.00:00:00", "0.00:05:00")]
        [TestCase("3.00:00:00", "0.00:05:00")]
        [TestCase("7.00:00:00", "0.00:10:00")]
        [TestCase("11.00:00:00", "0.00:15:00")]
        [TestCase("35.00:00:00", "0.00:30:00")]
        [TestCase("49.00:00:00", "0.01:00:00")]
        [TestCase("91.00:00:00", "0.02:00:00")]
        [TestCase("182.00:00:00", "0.04:00:00")]
        [TestCase("371.00:00:00", "0.12:00:00")]
        public void GetDateTimeInterval_DefaultInterval(string step, string expected)
        {
            var expectedTimeStamp = TimeSpan.Parse(expected);
            var stepTimeStamp = TimeSpan.Parse(step);
            var result =
                dateTimeIntervalService.GetDateTimeInterval(startDateTime, startDateTime.Add(stepTimeStamp));
            result.TimeSpan.Should().Be(expectedTimeStamp);
        }

        [Test]
        public void GetDateTimeInterval_StepIsTooLarge()
        {
            Action act = () => dateTimeIntervalService.GetDateTimeInterval(startDateTime, startDateTime.AddDays(372));
            act.Should().Throw<BadRequestException>().And.Message.Should()
                .Be("Bad request. Interval between start and end is too large");
        }

        [Test]
        public void GetDateTimeInterval_WithPreSelected_StepIsTooLarge()
        {
            Action act = () => dateTimeIntervalService.GetDateTimeInterval(startDateTime, startDateTime.AddDays(372), TimeSpan.FromDays(1));
            act.Should().Throw<BadRequestException>().And.Message.Should().Be("Bad request. Interval between start and end is too large");
        }

        [TestCase("1.00:00:00", "0.00:04:00")]
        [TestCase("3.00:00:00", "0.00:04:00")]
        [TestCase("7.00:00:00", "0.00:09:00")]
        [TestCase("11.00:00:00", "0.00:14:00")]
        [TestCase("35.00:00:00", "0.00:29:00")]
        [TestCase("49.00:00:00", "0.00:59:00")]
        [TestCase("91.00:00:00", "0.01:59:00")]
        [TestCase("182.00:00:00", "0.03:59:00")]
        [TestCase("371.00:00:00", "0.11:59:00")]
        public void GetDateTimeInterval_WithPreSelected_SelectedIntervalIsTooSmall(string step, string selected)
        {
            var selectedTimeStamp = TimeSpan.Parse(selected);
            var stepTimeStamp = TimeSpan.Parse(step);

            Action act = () => dateTimeIntervalService.GetDateTimeInterval(startDateTime, startDateTime.Add(stepTimeStamp), selectedTimeStamp);
            act.Should().Throw<BadRequestException>().And.Message.Should().Be("Bad request. Selected interval is too small");
        }

        [TestCase("1.00:00:00", "1.00:04:00")]
        [TestCase("3.00:00:00", "3.00:04:00")]
        [TestCase("7.00:00:00", "7.00:09:00")]
        [TestCase("11.00:00:00", "11.00:14:00")]
        [TestCase("35.00:00:00", "35.00:29:00")]
        [TestCase("49.00:00:00", "49.00:59:00")]
        [TestCase("91.00:00:00", "91.01:59:00")]
        [TestCase("182.00:00:00", "182.03:59:00")]
        public void GetDateTimeInterval_WithPreSelected_SelectedIntervalIsTooLarge(string step, string selected)
        {
            var selectedTimeStamp = TimeSpan.Parse(selected);
            var stepTimeStamp = TimeSpan.Parse(step);

            Action act = () => dateTimeIntervalService.GetDateTimeInterval(startDateTime, startDateTime.Add(stepTimeStamp), selectedTimeStamp);
            act.Should().Throw<BadRequestException>().And.Message.Should().Be("Bad request. Selected interval is too large");
        }

        [TestCase("1.00:00:00", "0.00:06:00")]
        [TestCase("3.00:00:00", "0.00:07:00")]
        [TestCase("7.00:00:00", "0.00:11:00")]
        [TestCase("11.00:00:00", "0.00:16:00")]
        [TestCase("35.00:00:00", "0.00:35:00")]
        [TestCase("49.00:00:00", "0.01:30:00")]
        [TestCase("91.00:00:00", "0.03:00:00")]
        [TestCase("182.00:00:00", "0.05:00:00")]
        [TestCase("371.00:00:00", "0.14:00:00")]
        public void GetDateTimeInterval_ReturnSelectedInterval(string step, string selected)
        {
            var selectedTimeStamp = TimeSpan.Parse(selected);
            var stepTimeStamp = TimeSpan.Parse(step);

            var result = dateTimeIntervalService.GetDateTimeInterval(startDateTime, startDateTime.Add(stepTimeStamp), selectedTimeStamp);
            result.TimeSpan.Should().Be(selectedTimeStamp);
        }
    }
}
