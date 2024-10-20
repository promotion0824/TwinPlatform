using System;
using System.Collections.Generic;
using System.Text;

namespace Willow.Common
{
    public interface IDateTimeService
    {
        DateTime UtcNow { get; }
    }
}
