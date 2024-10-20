using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Willow.Common
{
    public interface ICache
    {
        Task<T>  Get<T>(string key) where T : class;

        Task     Add<T>(string key, T objToAdd) where T : class;
        Task     Add<T>(string key, T objToAdd, DateTime dtExpires) where T : class;
        Task     Add<T>(string key, T objToAdd, TimeSpan tsExpires) where T : class;
                 
        Task     Remove(string key);    
    }
}
