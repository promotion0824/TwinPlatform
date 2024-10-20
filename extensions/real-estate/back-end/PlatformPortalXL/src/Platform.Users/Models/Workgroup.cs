using System;
using System.Collections.Generic;

namespace Willow.Platform.Users
{
    public class Workgroup : IUser
    {
        public Guid       Id        { get; set; }
        public string     Name      { get; set; }
        public Guid       SiteId    { get; set; }
        public List<Guid> MemberIds { get; set; }

        public UserType   Type      => UserType.Workgroup;
        public string     Email     => null;
        public string     FirstName => null;
        public string     LastName  => null;
        public string     Initials  => Name?.Substring(0, 1) ?? "";
    }
}
