using System;
using System.Threading.Tasks;

namespace Willow.Common
{
    public static class CacheExtensions 
    {
        /// <summary>
        /// Retrieve an item from the cache or create the item if it does not exist
        /// </summary>
        /// <param name="cache">Cache to query</param>
        /// <param name="key">Key of item to retrieve</param>
        /// <param name="fnCreate">Callback for creating the item if it does not exist in the cache</param>
        /// <param name="onError">A callback to call if there was an error adding the newly creating item into the cache</param>
        /// <param name="dtExpires">Explicit datetime to expire the item in the cache</param>
        /// <param name="tsExpires">Time relative to now to expire the cache. If dtExpires is valid then this value is ignored</param>
        /// <returns>Item returned from cache or created</returns>
        public static async Task<T> Get<T>(this ICache cache, string key, Func<Task<T>> fnCreate, Func<Exception, Task> onError = null, DateTime? dtExpires = null, TimeSpan? tsExpires = null) where T : class
        {
            T obj = null;
            
            try
            {
                obj = await cache.Get<T>(key).ConfigureAwait(false);
            }
            catch(Exception ex)
            {
                // Not in cache or other retrieval error
                if(onError != null)
                    await onError(ex);
            }

            if(obj == null)
            { 
                obj = await fnCreate().ConfigureAwait(false);

                // We can just fire and forget adding it to the cache
                _ = Task.Run( async ()=>
                {
                    try
                    { 
                        if(dtExpires != null)
                            await cache.Add(key, obj, dtExpires.Value);
                        else if(tsExpires != null)
                            await cache.Add(key, obj, tsExpires.Value);
                        else
                            await cache.Add(key, obj);
                    }
                    catch(Exception ex)
                    {
                        if(onError != null)
                            await onError(ex);
                    }

                }).ConfigureAwait(false);
            }

            return obj;
        }
    }   
}
