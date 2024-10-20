using Willow.Common;

namespace NotificationCore.Test.Infrastructure.MockServices;
public class MockDateTimeService : IDateTimeService
{
    public DateTime UtcNow { get; set; } = DateTime.UtcNow;
}
