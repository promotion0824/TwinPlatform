using PlatformPortalXL.Models;
using System;

namespace PlatformPortalXL.Dto
{
    public class InsightSourceDto
    {
        public Guid? SourceId { get; set; }
        public InsightSourceType SourceType { get; set; }
        public string SourceName { get; set; }     
	}
}
