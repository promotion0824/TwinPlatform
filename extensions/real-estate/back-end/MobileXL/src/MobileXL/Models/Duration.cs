using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MobileXL.Models
{
    /// <summary>
    /// Measure a duration of date/time
    /// </summary>
    public class Duration
    {
        public Duration()
        {

        }

        public Duration(string data)
        {
            if (!string.IsNullOrWhiteSpace(data))
            {
                var parts = data.Split(";");

                this.Units = int.Parse(parts[0]);
                this.UnitOfMeasure = (DurationUnit)int.Parse(parts[1]);
            }
        }

        public int Units { get; set; }
        public DurationUnit UnitOfMeasure { get; set; }

        public enum DurationUnit
        {
            Minute = 0,
            Day = 1,
            Week = 2,
            Month = 3,
            Year = 4
        }

        public override string ToString()
        {
            return $"{this.Units};{(int)this.UnitOfMeasure}";
        }
    }
}
