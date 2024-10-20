using System;
using Willow.Common;

namespace Willow.Tests.Infrastructure.MockServices
{
    public class MockDateTimeService : IDateTimeService
    {
        public DateTime UtcNow { get; set; } = DateTime.UtcNow;
    }
}