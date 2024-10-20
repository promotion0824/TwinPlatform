using System;

namespace DirectoryCore.Domain
{
    public static class UserTypeNames
    {
        public static string CustomerUser => "customeruser";
        public static string Supervisor => "supervisor";
        public static string Connector => "connector";
    }

    [Flags]
    public enum UserType
    {
        Customer = 1,
        Supervisor = 8,

        All = Customer | Supervisor,
        Unknown = All,
    }
}
