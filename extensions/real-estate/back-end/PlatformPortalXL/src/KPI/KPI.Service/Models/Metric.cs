using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Willow.KPI.Service
{
    public class Metric
    {
        public string          Name   { get; set; }
        public List<DataPoint> Values { get; set; } = new List<DataPoint>();
        public string          XUOM   { get; set; }
        public string          YUOM   { get; set; }
    }

    public class DataPoint
    {
        public object XValue { get; set; }
        public object YValue { get; set; }
    }
}
