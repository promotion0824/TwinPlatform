using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Xunit;

using Newtonsoft.Json;
using Willow.Common;
using Automobile = Willow.Common.UnitTests.ObjectExtensions.Automobile;

namespace Willow.Common.UnitTests
{
    public class StreamExtensions
    {
        [Fact]
        public async Task StreamExtensions_ReadObject()
        {
            using(var store = new MemoryStream(UTF8Encoding.Default.GetBytes(JsonConvert.SerializeObject(new Automobile { Make = "Chevy", Model = "Camaro", Year = 1969 } ))))
            { 
                var car = await store.ReadObject<Automobile>();

                Assert.NotNull(car);
                Assert.Equal("Chevy", car.Make);
                Assert.Equal("Camaro", car.Model);
                Assert.Equal(1969, car.Year);
            }
        }
    }
}
