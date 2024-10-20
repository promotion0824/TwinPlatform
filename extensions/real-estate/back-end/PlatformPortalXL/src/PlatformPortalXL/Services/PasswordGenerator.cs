using System;
using System.Security.Cryptography;

namespace PlatformPortalXL.Services
{
    public static class PasswordGenerator
    {
        private static readonly char[] Punctuations = "!@#$%^&*()_-+=[{]};:>|./?".ToCharArray();
        private static readonly char[] StartingChars = new [] { '<', '&' };
        private const int MinLengthThreshold = 1;
        private const int MaxLengthThreshold = 128;
        
        
        /// <summary>Generates a random password of the specified length.</summary>
        /// <returns>A random password of the specified length.</returns>
        /// <param name="length">The number of characters in the generated password. The length must be between 1 and 128 characters. </param>
        /// <param name="numberOfNonAlphanumericCharacters">The minimum number of non-alphanumeric characters (such as @, #, !, %, &amp;, and so on) in the generated password.</param>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="length" /> is less than 1 or greater than 128 -or-<paramref name="numberOfNonAlphanumericCharacters" /> is less than 0 or greater than <paramref name="length" />.
        /// </exception>
        public static string GeneratePassword(int length, int numberOfNonAlphanumericCharacters)
        {
            if (length < MinLengthThreshold || length > MaxLengthThreshold)
            {
                throw new ArgumentException("Password length is incorrect", nameof(length));
            }

            if (numberOfNonAlphanumericCharacters > length || numberOfNonAlphanumericCharacters < 0)
            {
                throw new ArgumentException("Minimal required number of non alphanumeric characters is incorrect", nameof(numberOfNonAlphanumericCharacters));
            }
            
            string s;
            do
            {
                var data = new byte[length];
                var chArray = new char[length];
                new RNGCryptoServiceProvider().GetBytes(data);
                var num1 = PopulateCharArray(length, data, chArray);
                if (num1 < numberOfNonAlphanumericCharacters)
                {
                    EnrichWithNonAlphanumericCharacters(length, numberOfNonAlphanumericCharacters, num1, chArray);
                }
                s = new string(chArray);
            }
            while (IsDangerousString(s));
            return s;
        }

        private static void EnrichWithNonAlphanumericCharacters(int length, int numberOfNonAlphanumericCharacters, int num1,
            char[] chArray)
        {
            for (var index1 = 0; index1 < numberOfNonAlphanumericCharacters - num1; ++index1)
            {
                int index2;
                do
                {
                    index2 = RandomNumberGenerator.GetInt32(0, length);
                } while (!char.IsLetterOrDigit(chArray[index2]));

                chArray[index2] = Punctuations[RandomNumberGenerator.GetInt32(0, Punctuations.Length)];
            }
        }

        private static int PopulateCharArray(int length, byte[] data, char[] chArray)
        {
            int num1 = 0;
            for (var index = 0; index < length; ++index)
            {
                var num2 = (int) data[index] % 87;
                if (num2 < 10)
                {
                    chArray[index] = (char) (48 + num2);
                }
                else if (num2 < 36)
                {
                    chArray[index] = (char) (65 + num2 - 10);
                }
                else if (num2 < 62)
                {
                    chArray[index] = (char) (97 + num2 - 36);
                }
                else
                {
                    chArray[index] = Punctuations[num2 - 62];
                    ++num1;
                }
            }

            return num1;
        }

        private static bool IsDangerousString(string s)
        {
            var i = 0;
            while(true)
            {
                // Look for the start of one of our patterns 
                var n = s.IndexOfAny(StartingChars, i);

                // If not found, the string is safe
                if (n < 0)
                {
                    return false;
                }

                // If it's the last char, it's safe 
                if (n == s.Length - 1)
                {
                    return false;
                }

                switch (s[n])
                {
                    case '<':
                        if (HtmlAlike(s, n))
                        {
                            return true;
                        }
                        break;
                    case '&':
                        // If the & is followed by a #, it's unsafe (e.g. &#83;) 
                        if (s[n + 1] == '#')
                        {
                            return true;
                        }
                        break;
                }
                // Continue searching
                i = n + 1;
            }
        }
        private static bool HtmlAlike(string s, int n)
        {
            // If the < is followed by a letter or '!', it's unsafe (looks like a tag or HTML comment)
            return IsAtoZ(s[n + 1]) || s[n + 1] == '!' || s[n + 1] == '/' || s[n + 1] == '?';
        }

        private static bool IsAtoZ(char c) => c >= 'a' && c <= 'z' || c >= 'A' && c <= 'Z';
    }
}
