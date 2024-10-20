using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Azure;
using Azure.Core;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using AzureBlobs = Azure.Storage.Blobs;

using Willow.Common;

namespace Willow.Azure.Storage
{
    /// <summary>
    /// Azure Blob Storage
    /// </summary>
    public class AzureBlobStorage : IBlobStore
    {
        private readonly string _connectionString;
        private readonly string _containerName;
        private readonly string _folderName = "";
        private readonly Uri    _uri;
        private readonly TokenCredential _credential;

        public AzureBlobStorage(string connectionString, string path, bool createContainer = true)
        {
           var parts = path.Replace("\\", "/").Split('/');

            _containerName    = parts[0];
            _connectionString = connectionString;

            if(parts.Length > 1)
                _folderName = string.Join("/", parts.Skip(1));

            if(!string.IsNullOrWhiteSpace(_folderName) && !_folderName.EndsWith("/"))
                _folderName += "/";

           if(createContainer)
                CreateContainerIfNotExists();
        }

        public AzureBlobStorage(Uri uri, TokenCredential credential, string path, bool createContainer = true)
        {
            _uri = uri;
            _credential = credential;

           if(!string.IsNullOrWhiteSpace(path))
            {
                if(!path.EndsWith("/"))
                    path += "/";

                _folderName = path;
            }

           if(createContainer)
                CreateContainerIfNotExists();
        }

        #region IBlobStore

        public Task Get(string id, Stream destination)
        {
            return DoBlobAction(id, async (blob)=>
            { 
                await blob.DownloadToAsync(destination).ConfigureAwait(false); 

                if(destination.CanSeek)
                    destination.Seek(0, SeekOrigin.Begin);
            });       
        }

        public Task Put(string id, Stream data, object tags)
        {
            return DoBlobAction(id, async (blob)=>
            { 
                if(data.CanSeek)
                    data.Seek(0, SeekOrigin.Begin);

                await blob.UploadAsync(data).ConfigureAwait(false);      

                if(tags != null)
                {
                    var dTags = tags.ToDictionary();

                    if(dTags.Count > 0)
                    { 
                        var newTags = new Dictionary<string, string>();

                        foreach(var kv in dTags)
                            if(kv.Value != null)
                                newTags.Add(kv.Key, kv.Value.ToString());
   
                        await blob.SetTagsAsync(newTags);
                    }
                }

            });       
        }
        
        public Task PutBlocks(string id, Func<Stream, Task<bool>> writeBlock, object tags = null)
        {
            var blobPath = new BlobPath();

            return DoActionAsync(blobPath, async (blobPath)=>
            { 
                var containerClient = GetContainerClient(out string path);

                await containerClient.CreateIfNotExistsAsync();

                var appendBlobClient = GetAppendBlobClient(id, true, out path);
                var shouldContinue   = true;
             
                blobPath.Path = path;

                do
                {
                    using(var stream = new MemoryStream())
                    { 
                        shouldContinue = await writeBlock(stream);

                        stream.Seek(0, SeekOrigin.Begin);

                        await appendBlobClient.AppendBlockAsync(stream);
                    }
                }
                while (shouldContinue);

                await appendBlobClient.SealAsync();
            }); 
        }

        public Task Delete(string id)
        {
            return DoBlobAction(id, async (blob)=>
            { 
                await blob.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots).ConfigureAwait(false);    
            });       
        }

        public Task Enumerate(Func<string, Task> fnEach, bool asynchronous = true, object matchingTags = null)
        {
            var blobPath = new BlobPath();

            return DoActionAsync(blobPath, async (blobPath)=>
            { 
                var        tags      = matchingTags?.ToDictionary();
                var        container = this.GetContainerClient(out string path);
                var        traits    = AzureBlobs.Models.BlobTraits.None;

                blobPath.Path = path;

                if(tags != null && tags.Count > 0)
                    traits = AzureBlobs.Models.BlobTraits.Tags;

                var        pages     = container.GetBlobsAsync(traits, prefix: _folderName).AsPages();
                List<Task> tasks     = asynchronous ? new List<Task>() : null;

                await foreach(var page in pages)
                {
                    var blobs = page.Values;

                    foreach(var blob in blobs)
                    {
                        if(!Matches(blob, tags))
                            continue;

                        var name = blob.Name.Substring(_folderName.Length);
                        var task = fnEach(name);

                        if(asynchronous)
                            tasks.Add(task);
                        else
                            await task.ConfigureAwait(false);
                    }
                }

                if(asynchronous)
                    await Task.WhenAll(tasks).ConfigureAwait(false);
            });
        }

        #endregion

        #region Private 
       
        private BlobClient GetBlobClient(string blobName, out string blobPath)
        { 
            BlobClient blob = null;

            if(_uri != null)
            { 
                blobPath = Path.Combine(_uri.ToString(), _folderName, blobName).Replace("\\", "/");
                var pageUri  = new Uri(blobPath);

                blob = new BlobClient(pageUri, _credential);
            }
            else
            { 
                blobPath = Path.Combine(_folderName, blobName).Replace("\\", "/");

                blob = new BlobClient(_connectionString, _containerName, blobPath);
            }

            return blob;
        }        
        
        private AppendBlobClient GetAppendBlobClient(string blobName, bool createIfNotExists, out string blobPath)
        { 
            AppendBlobClient blob = null;

            if(_uri != null)
            { 
                blobPath = Path.Combine(_uri.ToString(), _folderName, blobName).Replace("\\", "/");
                var pageUri  = new Uri(blobPath);

                return new AppendBlobClient(pageUri, _credential);
            }
            else
            { 
                blobPath = Path.Combine(_folderName, blobName).Replace("\\", "/");

                blob = new AppendBlobClient(_connectionString, _containerName, blobPath);
            }

            if(createIfNotExists)
                 blob.CreateIfNotExists();

            return blob;
        }

        private Task DoBlobAction(string id, Func<BlobClient, Task> fnAction)
        {
            var blobPath = new BlobPath();

            return DoActionAsync(blobPath, async (blobPath)=>
            { 
                var blob = GetBlobClient(id, out string path);
              
                blobPath.Path = path;

                await fnAction(blob).ConfigureAwait(false);      
            }); 
        }

        private class BlobPath
        {
            internal string Path { get; set; } = "";
        }

        private async Task DoActionAsync(BlobPath blobPath, Func<BlobPath, Task> fnAction)
        {
            try
            {             
                await fnAction(blobPath).ConfigureAwait(false);      
            }
            catch(RequestFailedException ex) when (ex.Status == 404)
            { 
                throw new FileNotFoundException("Blob not found", blobPath.Path, ex).WithData( new { ContainerName = _containerName });
            }
            catch(RequestFailedException ex2) when (ex2.Status == 500 && ex2.Message.Contains("Server failed to authenticate the request", StringComparison.InvariantCultureIgnoreCase))
            { 
                throw new UnauthorizedAccessException("Access denied", ex2).WithData( new { BlobPath = blobPath.Path, ContainerName = _containerName });
            }        
            catch(RequestFailedException ex3) when (ex3.Status == 403)
            { 
                throw new UnauthorizedAccessException("Access denied", ex3).WithData( new { BlobPath = blobPath.Path, ContainerName = _containerName });
            }  
            catch(RequestFailedException rex) when (rex.Status == 400)
            { 
                throw new ArgumentException(rex.Message).WithData( new { BlobPath = blobPath.Path, ContainerName = _containerName });
            }             
            catch(RequestFailedException rex2) 
            { 
                throw new Exception(rex2.Message).WithData( new { BlobPath = blobPath.Path, ContainerName = _containerName, StatusCode = rex2.Status });
            }  
        }        
        
        private void DoAction(BlobPath blobPath, Action<BlobPath> fnAction)
        {
            try
            {             
                fnAction(blobPath);
            }
            catch(RequestFailedException ex) when (ex.Status == 404)
            { 
                throw new FileNotFoundException("Blob not found", blobPath.Path, ex).WithData( new { ContainerName = _containerName });
            }
            catch(RequestFailedException ex2) when (ex2.Status == 500 && ex2.Message.Contains("Server failed to authenticate the request", StringComparison.InvariantCultureIgnoreCase))
            { 
                throw new UnauthorizedAccessException("Access denied", ex2).WithData( new { BlobPath = blobPath.Path, ContainerName = _containerName });
            }        
            catch(RequestFailedException ex3) when (ex3.Status == 403)
            { 
                throw new UnauthorizedAccessException("Access denied", ex3).WithData( new { BlobPath = blobPath.Path, ContainerName = _containerName });
            }  
            catch(RequestFailedException rex) when (rex.Status == 400)
            { 
                throw new ArgumentException(rex.Message).WithData( new { BlobPath = blobPath.Path, ContainerName = _containerName });
            }             
            catch(RequestFailedException rex2) 
            { 
                throw new Exception(rex2.Message).WithData( new { BlobPath = blobPath.Path, ContainerName = _containerName, StatusCode = rex2.Status });
            }  
        }

        private BlobContainerClient GetContainerClient(out string blobPath)
        { 
            if(_uri != null)
            { 
                blobPath = Path.Combine(_uri.ToString(), _folderName).Replace("\\", "/");

                var pageUri = new Uri(_uri.ToString());

                return new BlobContainerClient(pageUri, _credential);
            }

            return new BlobContainerClient(_connectionString, blobPath = _containerName);
        }

        private void CreateContainerIfNotExists()
        {
            var blobPath = new BlobPath();

            DoAction(blobPath, (blobPath)=>
            { 
                var container = GetContainerClient(out string path);

                blobPath.Path = path;

                container.CreateIfNotExists();
            });
        }

        private bool Matches(BlobItem blob, IDictionary<string, object> tags)
        {
            if(tags != null && tags.Count > 0)
            {
                if(blob.Tags == null)
                    return false;

                foreach(var tagName in tags.Keys)
                {
                    if(!blob.Tags.ContainsKey(tagName))
                        return false;

                    if(tags[tagName].ToString() != blob.Tags[tagName].ToString())
                        return false;
                }
            }

            return true;
        }

        #endregion
    }
}
