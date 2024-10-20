namespace Willow.Infrastructure.Services;

using System;

internal class DateTimeService : IDateTimeService
{
    public DateTime UtcNow => DateTime.UtcNow;
}
