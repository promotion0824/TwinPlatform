using System;
using System.Text;
using System.Security.Cryptography;

namespace PlatformPortalXL.Services.Sigma
{
    public static class SigmaHelper
    {
        public static string GetRandomNonce()
        {
            return Guid.NewGuid().ToString("N");
        }

        public static long GetUnixTime(this DateTime utcNow)
        {
            return new DateTimeOffset(utcNow).ToUnixTimeSeconds();
        }

        public static string Sign(string key, string payload)
        {
            byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);
            byte[] tokenBytes = Encoding.UTF8.GetBytes(key);
            using HMACSHA256 hmacSHA256 = new HMACSHA256(tokenBytes);
            byte[] hashBytes = hmacSHA256.ComputeHash(payloadBytes);
            var signature = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            return signature;
        }
    }
}
