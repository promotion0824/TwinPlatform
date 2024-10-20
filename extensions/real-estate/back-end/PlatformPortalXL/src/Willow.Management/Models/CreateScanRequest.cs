using System.ComponentModel.DataAnnotations;

namespace Willow.Management
{
    public class CreateScanRequest
    {
        private const int MinScanTimeLimit = 60;

        [Required(ErrorMessage = "WhoisSegmentSize is required")]
        [Range(1, int.MaxValue, ErrorMessage = "WhoisSegmentSize should be greater than 0")]
        public int WhoisSegmentSize { get; set; }

        [Required(ErrorMessage = "MinScanTime is required")]
        [Range(MinScanTimeLimit, int.MaxValue, ErrorMessage = "MinScanTime should be 60 or more")]
        public int MinScanTime { get; set; }

        [Required(ErrorMessage = "TimeInterval is required")]
        [Range(1, int.MaxValue, ErrorMessage = "TimeInterval should be greater than 0")]
        public int TimeInterval { get; set; }

        [Required(ErrorMessage = "InRangeOnly is required")]
        public bool InRangeOnly { get; set; }
    }
}
