using System;
using System.ComponentModel.DataAnnotations;
using PlatformPortalXL.Models;

namespace PlatformPortalXL.Requests.SiteCore
{
    public class CreateFloorRequest
    {
        const int CodeMaxLength = 10;
        [Required(ErrorMessage = "Floor name is required")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Floor code is required")]
        [StringLength(CodeMaxLength, ErrorMessage = "Floor code must not exceed a length of 10")]
        public string Code { get; set; }
        public string ModelReference { get; set; }
        public bool IsSiteWide { get; set; }   
    }
}
