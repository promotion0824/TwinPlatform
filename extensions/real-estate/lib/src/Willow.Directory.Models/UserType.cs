using System;

namespace Willow.Directory.Models
{
    [Flags]
    public enum UserType
    {
        Customer = 1,
        Workgroup = 4,
        Supervisor = 8,

        All = Customer | Workgroup,
        Unknown = All,
    }
}
