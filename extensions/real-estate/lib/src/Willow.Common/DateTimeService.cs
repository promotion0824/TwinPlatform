using System;
using System.Collections.Generic;
using System.Text;

namespace Willow.Common
{
    public class DateTimeService : IDateTimeService
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
