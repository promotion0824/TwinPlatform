using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Willow.Common
{
    public static class BlobStoreExtensions
    {
        /// <summary>
        /// Get the blob as a string
        /// </summary>
        /// <param name="store">IBlobStore to retrieve data from</param>
        /// <param name="id">An identifier for the blob. This could be a path</param>
        /// <param name="encoding">Text encoding. Must match how the text was originally saved.</param>
        /// <returns></returns>
        public static async Task<string> Get(this IBlobStore store, string id, Encoding encoding = null)
        {
            encoding ??= UTF8Encoding.UTF8;

            var array = await store.GetBytes(id);

            return encoding.GetString(array);
        }    

        /// <summary>
        /// Get the blob as an array of bytes
        /// </summary>
        /// <param name="store">IBlobStore to retrieve data from</param>
        /// <param name="id">An identifier for the blob. This could be a path</param>
        /// <returns></returns>
        public static async Task<byte[]> GetBytes(this IBlobStore store, string id)
        {
            using(var stream = new MemoryStream())
            { 
                await store.Get(id, stream);

                return stream.ToArray();
            }
        }    

        /// <summary>
        /// Get the blob as an object
        /// </summary>
        /// <param name="store">IBlobStore to retrieve data from</param>
        /// <param name="id">An identifier for the blob. This could be a path</param>
        /// <returns>The object read in from blob storage</returns>
        public static async Task<T> GetObject<T>(this IBlobStore store, string id)
        {
            using(var stream = new MemoryStream())
            { 
                await store.Get(id, stream);

                return await stream.ReadObject<T>();
            }
        }    

        /// <summary>
        /// Put the string as a blob into storage
        /// </summary>
        /// <param name="store">IBlobStore to retrieve data from</param>
        /// <param name="id">An identifier for the blob. This could be a path</param>
        /// <param name="encoding">Text encoding. Must match how the text was originally saved.</param>
        /// <returns></returns>
        public static async Task Put(this IBlobStore store, string id, string data, Encoding encoding = null)
        {
            encoding ??= UTF8Encoding.UTF8;

            byte[] array = encoding.GetBytes(data);

            using(var stream = new MemoryStream(array))
            { 
                await store.Put(id, stream);
            }
        } 
        /// <summary>
        /// Put the blob into storage if and only if no existing blob with matching tags exist
        /// </summary>
        /// <param name="store">IBlobStore to store blob</param>
        /// <param name="id">An identifier for the blob. This could be a path</param>
        /// <param name="content">Blob to save</param>
        /// <param name="tags">Tags to save with blob</param>
        /// <param name="matchTagNames">Names of tag to match against. If a blob exists with all tag values then the blob is not saved</param>
        /// <returns>The ids of the existing blobs or the new blob id</returns>
        public static async Task<IEnumerable<string>> PutIfTagsNotExist(this IBlobStore store, string id, Stream content, object tags, IEnumerable<string> matchTagNames)
        {
            var blobs     = new List<string>();
            var dTags     = tags.ToDictionary();
            var matchTags = new Dictionary<string, object>();

            foreach(var matchTagName in matchTagNames)
                if(dTags.ContainsKey(matchTagName))
                    matchTags.Add(matchTagName, dTags[matchTagName]);

            // Find all blobs with the matching tag
            await store.Enumerate((id)=>
            {
                blobs.Add(id);
                return Task.CompletedTask;
            },
            false,
            matchTags);

            // If blobs found then return the ids
            if(blobs.Count > 0)
                return blobs;

            // Store the requested blob
            await store.Put(id, content, tags);

            return new List<string> { id };
        }    
     }
}
