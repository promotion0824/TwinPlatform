using PlatformPortalXL.Models.PowerBI;
using System;

namespace PlatformPortalXL.Dto
{
    public class PowerBIReportTokenDto
    {
        public string Token { get; set; }
        public string Url { get; set; }
        public DateTime Expiration { get; set; }

        public static PowerBIReportTokenDto MapFrom(PowerBIReportToken reportToken)
        {
            return new PowerBIReportTokenDto
            {
                Token = reportToken.Token,
                Url = reportToken.Url,
                Expiration = reportToken.Expiration,
            };
        }
    }
}
