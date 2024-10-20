using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Willow.Common
{
    public class MemoryStore : IBlobStore
    {
        private readonly ConcurrentDictionary<string, Entry> _store = new ConcurrentDictionary<string, Entry>();

        public MemoryStore()
        {
        }

        public void Clear()
        {
            _store.Clear();
        }

        public Entry this[string key] => _store[key];
        public Entry this[int key]    => _store.ToList()[key].Value;
        public int    Count            => _store.Count;

        #region IBlobStore

        public Task Get(string id, Stream output)
        {
            if(!_store.ContainsKey(id))
                throw new FileNotFoundException();
                
            var result = _store[id].Content as byte[];

            return output.WriteAsync(result, 0, result.Length);
        }

        public Task Put(string id, Stream input, object tags = null)
        {
           _store[id] = new Entry { Id = id, Content = input.ToArray(), Tags = tags.ToDictionary() };

            return Task.CompletedTask;
        }

        public Task Delete(string id)
        {
            _store.TryRemove(id, out Entry _);

            return Task.CompletedTask;
        }

        public Task Enumerate(Func<string, Task> fnEach, bool asynchronous, object matchingTags)
        {
            var tags = matchingTags?.ToDictionary();

            foreach(var key in _store.Keys)
            { 
                if(tags != null)
                { 
                    var entry = _store[key];

                    if(entry.Tags != null)
                    {
                        foreach(var tagName in tags.Keys)
                        {
                            if(!entry.Tags.ContainsKey(tagName))
                                goto skip;

                            if(tags[tagName].ToString() != entry.Tags[tagName].ToString())
                                goto skip;
                        }
                    }
                }

                fnEach(key);

                skip:           
                    ;
            }

            return Task.CompletedTask;
        }

        public async Task PutBlocks(string id, Func<Stream, Task<bool>> writeBlock, object tags = null)
        {
            using(var stream = new MemoryStream())
            { 
                var shouldContinue = true;

                do
                {
                    shouldContinue = await writeBlock(stream);
                }
                while(shouldContinue);

                stream.Seek(0, SeekOrigin.Begin);

                await this.Put(id, stream, tags);
            }
        }

        #endregion

        public class Entry
        {
            public string Id { get; set; }
            public byte[] Content { get; set; }
            public IDictionary<string, object> Tags { get; set; }
        }
    }
}
