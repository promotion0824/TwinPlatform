using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Willow.Common
{
    public static class ObjectExtensions
    {
        public static IList<object> ToObjectList(this object obj)
        {
            if(obj is IList<object> list)
                return list;

            if(obj is IEnumerable enm)
            {
                var list2 = new List<object>();

                foreach(var item in enm)
                    list2.Add(item);

                return list2;
            }

            return null;
        }

        public static T GetValue<T>(this object obj, string propertyName)
        {
            var type    = obj.GetType();
            var property = type.GetProperty(propertyName);

            if(property == null)
                return default(T);

            var val = property.GetValue(obj);

            if(val != null && !val.GetType().IsEquivalentTo(typeof(T)))
                return (T)Convert.ChangeType(val, typeof(T));

            return (T)val;
        }

        /// <summary>
        /// Converts an object to a dictionary
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>A dictionary with properties of object</returns>
        public static IDictionary<string, object> ToDictionary(this object obj)
        {
            if (obj == null)
                return null;

            if (obj is IDictionary<string, object> dict)
                return dict;

            var result = new Dictionary<string, object>();

            if(obj is IDictionary dict2)
            {
                foreach(var key in dict2.Keys)
                    result.Add(key.ToString(), dict2[key]);
            }
            else if(obj is IEnumerable list)
            {
                foreach(var val in list)
                    result.Add(val?.ToString() ?? Guid.NewGuid().ToString(), val);
            }
            else
            {
                var properties = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where( p=> p.CanRead );

                foreach(var property in properties)
                {
                    try
                    { 
                        var value = property.GetGetMethod().Invoke(obj, null);

                        result.Add(property.Name, value);
                    }
                    catch
                    {
                        // Just ignore it
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Throws an exception with the Data property filled with the data param
        /// </summary>
        /// <param name="msg">Exception msg</param>
        /// <param name="data">An anonymous object, poco or dictionary with properties to be examined and/or logged downstream</param>

        public static void Throw<T>(this object _, string msg, object data) where T : Exception
        {
            var ex = Activator.CreateInstance(typeof(T), msg) as Exception;

            ex.Data.Merge(data.ToDictionary() as IDictionary);

            throw ex;
        }
    }
}
