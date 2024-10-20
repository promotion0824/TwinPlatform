using PlatformPortalXL.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System;

namespace PlatformPortalXL.ServicesApi.InsightApi
{
    public class BatchUpdateInsightStatusRequest
    {
        public List<Guid> Ids { get; set; }
        public InsightStatus Status { get; set; }
        public string Reason { get; set; }
        public Guid UpdatedByUserId { get; set; }
    }
}
