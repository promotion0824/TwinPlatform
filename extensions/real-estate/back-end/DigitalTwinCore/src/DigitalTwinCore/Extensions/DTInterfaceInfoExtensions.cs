using DTDLParser.Models;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace DTDLParser
{
    public static class DTInterfaceInfoExtensions
    {
        public static Guid GetUniqueId(this DTInterfaceInfo interfaceInfo) => ConvertStringToGuid(interfaceInfo.Id.AbsoluteUri);

        // https://github.com/dotnet/runtime/issues/61417
        // MD5 is not thread safe
        private static Guid ConvertStringToGuid(string value) => new Guid(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(value)));

        public static string GetPropertyDisplayName(this DTInterfaceInfo interfaceInfo, string name) 
        { 
            if (!string.IsNullOrWhiteSpace(name) && interfaceInfo.Contents.ContainsKey(name))
            {
                return interfaceInfo.Contents[name].GetDisplayName() ?? name;
            }

            return null;
        }

        public static string GetDisplayName(this DTEntityInfo entityInfo)
        {
            return entityInfo.DisplayName?.Values?.FirstOrDefault();
        }
    }
}