using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Xunit;
using Moq;

using Willow.Api.Client;
using Willow.Data.Rest;
using Willow.Common;

namespace Willow.Data.Rest.UnitTests
{
    public class RestRepositoryTests
    {
        private readonly Mock<IRestApi> _restApi = new Mock<IRestApi>();
        private readonly RestRepositoryReader<Guid, Automobile> _repo1;
        private readonly RestRepositoryReader<Guid, Automobile> _repo2;

        public RestRepositoryTests()
        {
            _repo1 = new RestRepositoryReader<Guid, Automobile>(_restApi.Object, (id)=> $"automobile/{id}", null);        
            _repo2 = new RestRepositoryReader<Guid, Automobile>(_restApi.Object, (id)=> $"automobile/{id}", null);        
        }

        [Fact]
        public async Task RestRepository_Get_success()
        {
            var id = Guid.NewGuid();

            _restApi.Setup( r=> r.Get<Automobile>($"automobile/{id}", It.IsAny<object>()) ).ReturnsAsync(new Automobile { Make = "Chevy", Model = "Corvette" });
            var result = await _repo1.Get(id);

            Assert.NotNull(result);
            Assert.Equal("Chevy", result.Make);
        }

        [Fact]
        public async Task RestRepository_GetList_success()
        {
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            var id3 = Guid.NewGuid();
            var id4 = Guid.NewGuid();

            _restApi.Setup( r=> r.Get<Automobile>($"automobile/{id1}", It.IsAny<object>()) ).ReturnsAsync(new Automobile { Id = id1, Make = "Chevy", Model = "Corvette" });
            _restApi.Setup( r=> r.Get<Automobile>($"automobile/{id2}", It.IsAny<object>()) ).ReturnsAsync(new Automobile { Id = id2, Make = "Pontiac", Model = "Firebird" });
            _restApi.Setup( r=> r.Get<Automobile>($"automobile/{id3}", It.IsAny<object>()) ).ReturnsAsync(new Automobile { Id = id3, Make = "Ford", Model = "Cobra" });
            _restApi.Setup( r=> r.Get<Automobile>($"automobile/{id4}", It.IsAny<object>()) ).ReturnsAsync(new Automobile { Id = id4, Make = "Dodge", Model = "Charger" });
                                                                        
            var result = await _repo1.Get(new List<Guid> { id1, id2, id3} ).ToList();

            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.True(result.Where( c=> c.Make == "Chevy").Any());
            Assert.True(result.Where( c=> c.Make == "Pontiac").Any());
            Assert.True(result.Where( c=> c.Make == "Ford").Any());
        }

        [Fact]
        public async Task RestRepository_Get_notfound()
        {
            var id = Guid.NewGuid();

            _restApi.Setup( r=> r.Get<Automobile>($"automobile/{id}", It.IsAny<object>()) ).ReturnsAsync(new Automobile { Make = "Chevy", Model = "Corvette" });
            await Assert.ThrowsAsync<Exception>( async ()=> await _repo1.Get(Guid.NewGuid()));
        }
    }

    public class Automobile 
    {
        public Guid     Id    {get; set;}
        public string   Make  {get; set;}
        public string   Model {get; set;}
        public string   Color {get; set;}
        public int      Year  {get; set;} = 1964;
    }
}
