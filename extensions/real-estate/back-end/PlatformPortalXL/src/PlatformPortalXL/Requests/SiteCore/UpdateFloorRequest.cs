using PlatformPortalXL.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace PlatformPortalXL.Requests.SiteCore
{
    public class UpdateFloorRequest
    {
        const int CodeMaxLength = 10;
        public string Name { get; set; }
        [StringLength(CodeMaxLength, ErrorMessage = "Floor code must not exceed a length of 10")]
        public string Code { get; set; }
        public string ModelReference { get; set; } 
        public bool? IsSiteWide { get; set; } 
    }
}
