using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xunit;
using Newtonsoft.Json;

using Willow.Common;
using Automobile = Willow.Common.UnitTests.ObjectExtensions.Automobile;

namespace Willow.Common.UnitTests
{
    public class BlobStoreExtensions
    {
        [Fact]
        public async Task BlobStoreExtensions_Put()
        {
            IBlobStore store = new MemoryStore();

            await store.Put("bob", "Bobs your uncle!");
        }

        [Fact]
        public async Task BlobStoreExtensions_Get()
        {
            IBlobStore store = new MemoryStore();

            await store.Put("bob", "Bobs your uncle!");
            await store.Put("fred", "Freds your uncle!");
            await store.Put("wilma", "Wilma's your aunt!");

            Assert.Equal("Bobs your uncle!", await store.Get("bob"));
            Assert.Equal("Freds your uncle!", await store.Get("fred"));
            Assert.Equal("Wilma's your aunt!", await store.Get("wilma"));
        }

        [Fact]
        public async Task BlobStoreExtensions_GetObject()
        {
            IBlobStore store = new MemoryStore();

            using(var stream = new MemoryStream(UTF8Encoding.Default.GetBytes(JsonConvert.SerializeObject(new Automobile { Make = "Chevy", Model = "Camaro", Year = 1969 } ))))
            { 
                await store.Put("bob", stream);

                var car = await store.GetObject<Automobile>("bob");

                Assert.NotNull(car);
                Assert.Equal("Chevy", car.Make);
                Assert.Equal("Camaro", car.Model);
                Assert.Equal(1969, car.Year);
            }
        }

        [Fact]
        public async Task BlobStoreExtensions_PutIfTagsNotExist()
        {
            IBlobStore store = new MemoryStore();
            var autoContent = UTF8Encoding.Default.GetBytes(JsonConvert.SerializeObject(new Automobile { Make = "Chevy", Model = "Camaro", Year = 1969 } ));

            using(var stream = new MemoryStream(autoContent))
            { 
                await store.Put("bob", stream, new { Owner = "Fred" } );
            }

            IList<string> ids = null;

            using(var stream2 = new MemoryStream(autoContent))
            { 
                ids = (await store.PutIfTagsNotExist("george", stream2, new { Owner = "Fred" }, new string[] { "Owner" }  )).ToList();
            }

            Assert.NotEmpty(ids);
            Assert.Equal("bob", ids.First());
        }
    }
}
