using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Willow.Common
{
    public static class DictionaryExtensions
    {
        public static IDictionary Merge(this IDictionary dest, IDictionary src)
        {
            foreach(var key in src.Keys)
                dest[key] = src[key];

            return dest;
        }
    }
}
