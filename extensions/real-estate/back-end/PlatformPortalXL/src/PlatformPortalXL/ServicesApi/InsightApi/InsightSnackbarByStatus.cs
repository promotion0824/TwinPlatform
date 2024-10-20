using System;
using PlatformPortalXL.Models;

namespace PlatformPortalXL.ServicesApi.InsightApi
{
    public class InsightSnackbarByStatus
    {
        public Guid? Id { get; set; }
        public InsightStatus Status { get; set; }
        public int Count { get; set; }
        public InsightSourceType? SourceType { get; set; }
        public string SourceName { get; set; }
    }
}
