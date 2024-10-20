using System;
using System.Globalization;

namespace DirectoryCore.Domain
{
    public static class WellKnownUsers
    {
        public static class CustomerSupport
        {
            public static string FirstName => "Support";
            public static string LastName => "Willow";
            public static string Initials => "SW";
            public static string Email => "support@willowinc.com";

            public static string UserName(Guid customerId) =>
                customerId.ToString("N", CultureInfo.InvariantCulture) + "@support.willowinc.com";
        }

        public static class DeletedUser
        {
            public static string FirstName => "Unknown";
            public static string LastName => "Unknown";

            public static string Email(string emailAddress) => $"deleted.{emailAddress}";

            public static string InactiveName(string name) => $"{name}(Inactive)";
        }
    }
}
