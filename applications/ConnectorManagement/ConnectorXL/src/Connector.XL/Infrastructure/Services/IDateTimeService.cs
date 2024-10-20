namespace Willow.Infrastructure.Services;

using System;

internal interface IDateTimeService
{
    DateTime UtcNow { get; }
}
