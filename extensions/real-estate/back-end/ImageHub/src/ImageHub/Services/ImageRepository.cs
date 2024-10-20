using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ImageHub.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

using Willow.Common;

namespace ImageHub.Services
{
    public interface IImageRepository
    {
        Task<ImageDescriptor> Add(string imagePath, Stream fileContent);
        Task<Stream>          Get(string imagePath);
        Task                  Delete(string imagePath);
        Task                  DeleteFolder(string imagePath);
    }

    public class ImageRepository : IImageRepository
    {
        private readonly IBlobStore _blobStore;

        public ImageRepository(IBlobStore blobStore)
        {
            _blobStore   = blobStore;
        }

        #region IImageRepository

        public async Task<Stream> Get(string imagePath)
        {
            var dataStream = new MemoryStream();
            
            try
            { 
                await _blobStore.Get(imagePath, dataStream);
            }
            catch
            {
                dataStream.Dispose();
                throw;
            }

            return dataStream;
        }

        public async Task<ImageDescriptor> Add(string imagePath, Stream fileContent)
        {
            await _blobStore.Put(imagePath, fileContent);

            return new ImageDescriptor
            {
                FileName      = imagePath,
                FileExtension = Path.GetExtension(imagePath).TrimStart('.')
            };
        }

        public Task Delete(string imagePath)
        {
            return _blobStore.Delete(imagePath);
        }

        public async Task DeleteFolder(string folder)
        {
           await _blobStore.Enumerate( async (filePath)=>
           {
               if(!filePath.StartsWith(folder, StringComparison.InvariantCultureIgnoreCase))
                   return;

                try
                {
                    await _blobStore.Delete(filePath);
                }
                catch(FileNotFoundException)
                {
                    // Do nothing
                }           
           },
           true);
        }

        #endregion
    }
}
