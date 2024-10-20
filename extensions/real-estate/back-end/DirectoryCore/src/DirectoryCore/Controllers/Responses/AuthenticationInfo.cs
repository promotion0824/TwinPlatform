using System;
using DirectoryCore.Dto;

namespace DirectoryCore.Controllers.Responses
{
    public class AuthenticationInfo
    {
        public string AccessToken { get; set; }
        public int ExpiresIn { get; set; }
        public string UserType { get; set; }

        [Obsolete("Use CustomerUser property instead")]
        public UserDto User => CustomerUser;
        public UserDto CustomerUser { get; set; }
        public SupervisorDto Supervisor { get; set; }
        public string RefreshToken { get; set; }
    }
}
