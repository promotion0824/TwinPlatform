using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Text.RegularExpressions;

using System.ComponentModel.DataAnnotations;

namespace Willow.DataValidation
{
    [System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Parameter | System.AttributeTargets.Property, AllowMultiple=false)]
    public class EmailAttribute : ValidationAttribute
    {
        private static Regex _localPartRestrictedRegEx = new Regex(@"[(),:;<>\[\\\]]", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
        private static Regex _domainNameRegEx = new Regex(@"[a-z0-9.-]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);

        public EmailAttribute()
        {
        }

        public override bool IsValid(object value)
        {
            if(value == null)
                return true;

            var emailAddress = value.ToString().Trim();

            if(emailAddress.Length == 0 || emailAddress.Length > 254)
                return false;

            var parts = emailAddress.Split("@");

            if(parts.Length != 2)
                return false;

            if(!ValidateLocalPart(parts[0]))
                return false;

            return ValidateDomain(parts[1]);
        }
        
        private bool ValidateLocalPart(string localPart)
        {
            if(!ValidateCommon(localPart, 64))
                return false;

            // Valid characters !#$%&'*+-/=?^_`{|}~
            // Restricted characters (),:;<>@[\] (+ space)

            if(localPart.Where( c=> char.IsLetterOrDigit(c) || ".!#$%&'*+-/=?^_`{|}~(),:;<>@[\\] \"".Contains(c) ).Count() != localPart.Length)
                return false;

            // Restricted characters are allowed inside double quotes only
            if(_localPartRestrictedRegEx.Match(localPart).Length != 0)
            { 
                var numQuotes = localPart.Where( c=> c == '"').Count();

                // Must be an even number of double quotes
                if(numQuotes == 0 || (localPart.Where( c=> c == '"').Count() | 1) == 1)
                    return false;

                var parts = localPart.Split("\"", StringSplitOptions.RemoveEmptyEntries);
                var start = localPart.StartsWith("\"") ? 1 : 0;

                for(var i = start; i < parts.Count(); i += 2)
                    if(_localPartRestrictedRegEx.Match(parts[i]).Length != 0)
                        return false;
            }
    
            return true;
        }
        
        private bool ValidateDomain(string domain)
        {
            if(!ValidateCommon(domain, 253))
                return false;
                        
            if(domain.StartsWith("-") || domain.EndsWith("-") || domain.Contains(".."))
                return false;

            var results = _domainNameRegEx.Match(domain).Value;

            if(results != domain)
                return false;

            var parts = domain.Split(".");

            if(parts.Length < 2 || parts[parts.Length-1].Length < 2)
                return false;

            return true;
        }

        private bool ValidateCommon(string part, int maxLength)
        {
            if(part.Length > maxLength)
                return false;

            if(part.Contains(".."))
                return false;

            if(part.StartsWith(".") || part.EndsWith("."))
                return false;

            return true;
        }
        
    }
}
