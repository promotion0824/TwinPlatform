using System;
using System.Threading.Tasks;
using Xunit;

using Microsoft.Extensions.Caching.Memory;

using Moq;

using Willow.Data;

namespace Willow.Data.UnitTests
{
    public class CachedRepositoryTests
    {
        private readonly Mock<IReadRepository<SiteObjectIdentifier, User>> _userRepo = new Mock<IReadRepository<SiteObjectIdentifier, User>>();
        private readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
        private readonly CachedRepository<SiteObjectIdentifier, User> _cachedRepo;
        private readonly Guid _siteId = Guid.NewGuid();

        public CachedRepositoryTests()
        {
            _cachedRepo = new CachedRepository<SiteObjectIdentifier, User>(_userRepo.Object, _cache, TimeSpan.FromMinutes(10), "bob_");
        }

        [Fact]
        public async Task CachedRepository_Get_SiteIdentifier()
        {
            var userId = Guid.NewGuid();
            var user = new User { SiteId = _siteId, Id = userId, FirstName = "Fred", LastName = "Flintstone", Email = "duh", Phone = "123" } ;
            var key = "bob_" + _siteId.ToString() + "_" + userId.ToString();

            _userRepo.Setup( r=> r.Get(It.Is<SiteObjectIdentifier>( p=> p.SiteId == _siteId && p.Id == userId))).ReturnsAsync(user);

            var result = await _cachedRepo.Get(_siteId, userId);

            Assert.Equal(user, result);

            _userRepo.Verify( r=> r.Get(It.Is<SiteObjectIdentifier>( p=> p.SiteId == _siteId && p.Id == userId)), Times.Once);

            result = await _cachedRepo.Get(_siteId, userId);

            Assert.Equal(user, result);

            _userRepo.Verify( r=> r.Get(It.Is<SiteObjectIdentifier>( p=> p.SiteId == _siteId && p.Id == userId)), Times.Once);
        }
    }
}
