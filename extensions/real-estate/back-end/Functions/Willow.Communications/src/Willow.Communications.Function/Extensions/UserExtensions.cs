
using System;
using Willow.Directory.Models;

namespace Willow.Communications.Function.Extensions;
public static class UserExtensions
{
    public static UserType ToUserType(this string userType)
    {
        if (string.IsNullOrEmpty(userType) || string.Compare(userType, "customeruser", true) == 0)
        {
            return UserType.Customer;
        }

        if (Enum.TryParse(userType, true, out UserType ut))
        {
            return ut;
        }

        return UserType.Unknown;
    }
}
