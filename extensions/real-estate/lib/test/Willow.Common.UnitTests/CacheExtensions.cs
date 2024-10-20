using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Xunit;
using Moq;

using Willow.Common;

namespace Willow.Common.UnitTests
{
    public class CacheExtensionsTests
    {
        private readonly Mock<ICache> _cacheMock = new Mock<ICache>();
        private readonly ICache       _cache;

        public CacheExtensionsTests()
        {
            _cache = _cacheMock.Object;
        }

        [Fact]
        public async Task CacheExtensions_Get_nocache()
        {
            var result = await _cache.Get<string>("bob", async ()=>
            {
                return await Task.FromResult("fred");
            });

            await Task.Delay(100);

            Assert.Equal("fred", result);
            _cacheMock.Verify( c=> c.Add("bob", "fred"), Times.Once);
            _cacheMock.Verify( c=> c.Add(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<DateTime>()), Times.Never);
            _cacheMock.Verify( c=> c.Add(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<TimeSpan>()), Times.Never);
        }

        [Fact]
        public async Task CacheExtensions_Get_cached()
        {
            _cacheMock.Setup(x => x.Get<string>("bob")).ReturnsAsync("wilma");

            var result = await _cache.Get<string>("bob", async ()=>
            {
                return await Task.FromResult("fred");
            });

            await Task.Delay(100);

            Assert.Equal("wilma", result);
            _cacheMock.Verify( c=> c.Add(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
            _cacheMock.Verify( c=> c.Add(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<DateTime>()), Times.Never);
            _cacheMock.Verify( c=> c.Add(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<TimeSpan>()), Times.Never);
        }

        [Fact]
        public async Task CacheExtensions_Get_cache_get_error()
        {
            _cacheMock.Setup(x => x.Get<string>("bob")).ThrowsAsync(new Exception());

            var result = await _cache.Get<string>("bob", async ()=>
            {
                return await Task.FromResult("fred");
            });

            await Task.Delay(100);

            Assert.Equal("fred", result);
            _cacheMock.Verify( c=> c.Add("bob", "fred"), Times.Once);
            _cacheMock.Verify( c=> c.Add(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<DateTime>()), Times.Never);
            _cacheMock.Verify( c=> c.Add(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<TimeSpan>()), Times.Never);
        }

        [Fact]
        public async Task CacheExtensions_Get_cache_add_error()
        {
            _cacheMock.Setup(x => x.Add("bob", "fred")).ThrowsAsync(new UnauthorizedAccessException());

            var error = "";

            var result = await _cache.Get<string>("bob", async ()=>
            {
                return await Task.FromResult("fred");
            },
            (Exception ex)=>
            {
                error = "Ouch!";
                Assert.True(ex is UnauthorizedAccessException);
                return Task.CompletedTask;
            });

            await Task.Delay(100);

            Assert.Equal("fred", result);
            Assert.Equal("Ouch!", error);
        }
    }
}
