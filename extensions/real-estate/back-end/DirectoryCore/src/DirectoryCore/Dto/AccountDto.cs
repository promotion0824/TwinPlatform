using System;

namespace DirectoryCore.Dto
{
    public class AccountDto
    {
        public string Email { get; set; }
        public string UserType { get; set; }
        public Guid UserId { get; set; }
    }
}
