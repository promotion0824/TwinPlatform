using System;
using System.ComponentModel.DataAnnotations;

using Willow.DataValidation;

namespace PlatformPortalXL.ServicesApi.DirectoryApi
{
    public abstract class UserRequest
    {
        [HtmlContent]
        [StringLength(50)]
        [Required]
        public string FirstName { get; set; }

        [HtmlContent]
        [StringLength(50)]
        [Required]
        public string LastName { get; set; }
    }

    public abstract class CustomerUserRequest : UserRequest
    {
        [Phone]
        [StringLength(50)]
        [Required]
        public string Mobile { get; set; }

        [HtmlContent]
        [Required]
        [StringLength(100)]
        public string Company { get; set; }
    }

    public class DirectoryCreateCustomerUserRequest : CustomerUserRequest
    {
        [Email]
        [Required]
        [StringLength(100)]
        public string Email { get; set; }

        public bool UseB2C { get; set; }
    }

    public class DirectoryUpdateCustomerUserRequest : CustomerUserRequest
    {
    }
}
