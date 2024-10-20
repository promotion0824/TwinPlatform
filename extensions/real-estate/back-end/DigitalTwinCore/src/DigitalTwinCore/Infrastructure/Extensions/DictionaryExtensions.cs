
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System.Linq;

namespace System.Collections.Generic
{
    public static class DictionaryExtensions
    {
        public static TV GetValueOrDefault<TK,TV>(this IDictionary<TK,TV> d, TK k, TV def = default)
        {
            return (k is not null) ? (d.ContainsKey(k) ? d[k] : def) : def;
        }

        public static string GetStringValueOrDefault<TK,TV>(this IDictionary<TK,TV> d, TK k, string def = default)
        {
            return GetValueOrDefault(d, k) as string ?? def;
        }

        public static bool GetBoolValueOrDefault<TK, TV>(this IDictionary<TK, TV> d, TK k, bool def = false)
        {
            return GetValueOrDefault(d, k) switch
            {
                bool b => b,
                _ => def
            };
        }

        public static int GetIntValueOrDefault<TK, TV>(this IDictionary<TK, TV> d, TK k, int def = 0)
        {
            return GetValueOrDefault(d, k) switch
            {
                decimal dec => (int) dec, // Note: existing code allowed truncating from a decimal, but not a float
                double dbl => (int)dbl,
                long l => (int)l,
                int i => i,
                _ => def
            };
        }

        public static float GetFloatValueOrDefault<TK, TV>(this IDictionary<TK, TV> d, TK k, float def = 0.0f)
        {
            return GetValueOrDefault(d, k) switch
            {
                float f => f,
                decimal dec => (float) dec,
                double dbl => (float) dbl,
                int i => i,
                long l => l, // Note: When using the System.Text.Json to serialize the twin and value is zero decimal place, e.g. 1.0. it truncates the demcial parts and read as long type  
                _ => def
            };
        }

        public static object GetValueOrDefaultIgnoreCase(this IDictionary<string, object> d, string k)
        {
            return GetValueOrDefault(d, k) ??
                    GetValueOrDefault(d, d.Keys.FirstOrDefault(x => x.Equals(k, StringComparison.OrdinalIgnoreCase)));
        }
    }
}
