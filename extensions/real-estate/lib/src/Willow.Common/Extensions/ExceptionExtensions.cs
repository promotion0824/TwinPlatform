using System;
using System.Collections;

namespace Willow.Common
{
    public static class ExceptionExtensions
    {
        /// <summary>
        /// Provides additional context to an exception by converting the passed in data to a Dictionary and merging it into the exception's Data
        /// </summary>
        /// <param name="data">An anonymous object, poco or dictionary with properties to be examined and/or logged downstream</param>
        public static T WithData<T>(this T ex, object data) where T : Exception
        {
            ex.Data.Merge(data.ToDictionary() as IDictionary);

            return ex;
        }

        /// <summary>
        /// Provides additional context to an exception by appending the passed in data to the exception's Data under the given name
        /// </summary>
        /// <param name="name">A string representing the name/key under which the data is stored </param>
        /// <param name="data">An anonymous object, poco or dictionary with properties to be examined and/or logged downstream</param>
        public static T WithData<T>(this T ex, string name, object value) where T : Exception
        {
            ex.Data[name] = value;

            return ex;
        }
    }
}
