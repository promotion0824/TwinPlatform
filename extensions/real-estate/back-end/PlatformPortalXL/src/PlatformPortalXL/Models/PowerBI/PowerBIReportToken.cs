using System;

namespace PlatformPortalXL.Models.PowerBI
{
    public class PowerBIReportToken
    {
        public string Token { get; set; }
        public string Url { get; set; }
        public DateTime Expiration { get; set; }
    }
}
