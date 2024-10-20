using System.Threading.Tasks;
using DirectoryCore.Domain;
using DirectoryCore.Dto;
using DirectoryCore.Services;
using DirectoryCore.Services.Auth0;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Willow.Infrastructure.Exceptions;

namespace DirectoryCore.Controllers
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class AccountsController : ControllerBase
    {
        private readonly IAuth0ManagementService _auth0Management;
        private readonly ICustomerUsersService _customerUserService;

        public AccountsController(
            IAuth0ManagementService auth0Management,
            ICustomerUsersService customerUserService
        )
        {
            _auth0Management = auth0Management;
            _customerUserService = customerUserService;
        }

        [HttpGet("accounts/{email}")]
        [Authorize]
        [ProducesResponseType(typeof(AccountDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAccount(string email)
        {
            var (userType, userId) = await _auth0Management.GetUserInfo(email);
            if (string.IsNullOrEmpty(userType))
            {
                var customerUser = await _customerUserService.GetCustomerUserByEmail(email);
                if (customerUser != null)
                {
                    userType = UserTypeNames.CustomerUser;
                    userId = customerUser.Id;
                }
            }
            if (string.IsNullOrEmpty(userType))
            {
                throw new ResourceNotFoundException("account", email);
            }
            var account = new AccountDto
            {
                Email = email,
                UserType = userType,
                UserId = userId
            };
            return Ok(account);
        }
    }
}
