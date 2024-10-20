using InsightCore.Models;
using System.ComponentModel.DataAnnotations;

namespace InsightCore.Controllers.Requests
{
    public class UpdateInsightStateRequest
    {
        [EnumDataType(typeof(InsightState))]
        public InsightState State { get; set; }
    }
}
