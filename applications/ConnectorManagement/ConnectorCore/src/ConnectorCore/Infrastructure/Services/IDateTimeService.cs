namespace Willow.Infrastructure.Services
{
    using System;

    internal interface IDateTimeService
    {
        DateTime UtcNow { get; }
    }

    internal class DateTimeService : IDateTimeService
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
