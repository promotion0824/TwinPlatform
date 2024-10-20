using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using Willow.DataValidation;

namespace Willow.Management
{
    public abstract class ManagedUserRequest
    {
        [HtmlContent(ErrorMessage = "First name contains invalid characters")]
        [StringLength(50, ErrorMessage = "First name must not exceed a length of 50")]
        [Required(ErrorMessage = "First name is required")]
        public string FirstName { get; set; }

        [HtmlContent(ErrorMessage = "Last name contains invalid characters")]
        [StringLength(50, ErrorMessage = "Last name must not exceed a length of 50")]
        [Required(ErrorMessage = "Last name is required")]
        public string LastName { get; set; }

        [HtmlContent(ErrorMessage = "Company contains invalid characters")]
        [Required(ErrorMessage = "Company is required")]
        [StringLength(100, ErrorMessage = "Company must not exceed a length of 100")]
        public string Company { get; set; }

        [Phone(ErrorMessage = "Contact number is invalid")]
        [StringLength(50, ErrorMessage = "Contact number must not exceed a length of 50")]
        [Required(ErrorMessage = "Contact number is required")]
        public string ContactNumber { get; set; }

        [Required(ErrorMessage = "Role is required")]
        public bool?  IsCustomerAdmin { get; set; }

        public List<ManagedPortfolioDto> Portfolios { get; set; } = new List<ManagedPortfolioDto>();
    }

    public class CreateManagedUserRequest : ManagedUserRequest
    {
        [Email(ErrorMessage = "Email is an invalid format")]
        [Required(ErrorMessage = "Email is required")]
        [StringLength(100, ErrorMessage = "Email must not exceed a length of 100")]
        public string Email { get; set; }
    }

    public class UpdateManagedUserRequest : ManagedUserRequest
    {
    }
}
