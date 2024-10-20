using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Willow.Common
{
    public interface IBlobStore
    {
        /// <summary>
        /// Copies a blob from storage with the given id/path to the given stream
        /// </summary>
        /// <param name="id">An identifier for the blob. This could be a path.</param>
        /// <param name="destination">Destination stream to write blob to</param>
        Task Get(string id, Stream destination);    

        /// <summary>
        /// Puts the stream into the blob storage
        /// </summary>
        /// <param name="id">An identifier for the blob. This could be a path in file storage for instance</param>
        /// <param name="content">The content to store</param>
        /// <param name="tags">Optional set of tags to associate with this blob</param>
        Task Put(string id, Stream content, object tags = null);    

       /// <summary>
        /// Write a series of blocks into the blob storage
        /// </summary>
        /// <param name="id">An identifier for the blob. This could be a path in file storage for instance</param>
        /// <param name="writeBlock">A function to write a block contents with. This function should return to continue or false to stop writing</param>
        /// <param name="tags">Optional set of tags to associate with this blob</param>
        Task PutBlocks(string id, Func<Stream, Task<bool>> writeBlock, object tags = null);    

        /// <summary>
        /// Deletes the blob from storage
        /// </summary>
        /// <param name="id">An identifier for the blob. This could be a path in file storage for instance</param>
        Task Delete(string id);
 
        /// <summary>
        /// Enumerates each blob and calls the given method for each
        /// </summary>
        /// <param name="fnEach">A function to call with each blob</param>
        /// <returns></returns>
        Task Enumerate(Func<string, Task> fnEach, bool asynchronous = true, object matchingTags = null);   
    }
}
