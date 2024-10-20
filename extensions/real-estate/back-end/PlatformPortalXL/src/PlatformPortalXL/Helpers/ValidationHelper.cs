using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PlatformPortalXL.Helpers
{
    public static class ValidationHelper
    {
        public static bool IsEmailValid(string email)
        {
            const string pattern = @"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$";

            return Regex.IsMatch(email, pattern, RegexOptions.Compiled | RegexOptions.Singleline);
        }

        public static bool IsPhoneNumberValid(string phoneNumber)
        {
            const string pattern = @"^(?=(?:\D*\d){10,15}\D*$)\+?[0-9]{1,3}[\s-]?(?:\(0?[0-9]{1,5}\)|[0-9]{1,5})[-\s]?[0-9][\d\s-]{5,7}\s?(?:x[\d-]{0,4})?$";

            return Regex.IsMatch(phoneNumber, pattern, RegexOptions.Compiled | RegexOptions.Singleline);
        }

        public static bool EqualsDefaultValue<T>(T value)
        {
            return EqualityComparer<T>.Default.Equals(value, default(T));
        }
    }
}
