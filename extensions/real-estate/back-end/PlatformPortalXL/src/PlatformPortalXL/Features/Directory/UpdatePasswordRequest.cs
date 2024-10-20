using System.ComponentModel.DataAnnotations;
using Willow.DataValidation;

namespace PlatformPortalXL.Features.Directory
{
    public class EmailRequest
    {
        [Email]
        [Required(AllowEmptyStrings = false)]
        public string Email    { get; set; }
    }

    public class OldUpdatePasswordRequest 
    {
        public string Token    { get; set; }
        public string Password { get; set; }
    }

    public class UpdatePasswordRequest : EmailRequest
    {
        public string Token    { get; set; }
        public string Password { get; set; }
    }
}
