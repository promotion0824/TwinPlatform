using System;

using Newtonsoft.Json.Converters;

namespace Willow.Calendar
{
    public class DateFormatConverter : IsoDateTimeConverter
    {
        public DateFormatConverter(string format)
        {
            DateTimeFormat = format;
        }
    }
}
