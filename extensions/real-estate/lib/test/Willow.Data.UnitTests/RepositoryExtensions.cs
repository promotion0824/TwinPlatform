using System;
using System.Threading.Tasks;
using Xunit;

using Moq;

using Willow.Data;

namespace Willow.Data.UnitTests
{
    public class RepositoryExtensionsTests
    {
        private readonly Mock<IReadRepository<SiteObjectIdentifier, User>> _userRepo = new Mock<IReadRepository<SiteObjectIdentifier, User>>();
        private readonly Guid _siteId = Guid.NewGuid();

        [Fact]
        public async Task RepositoryExtensions_Get_SiteIdentifier()
        {
            var userId = Guid.NewGuid();
            var user = new User { SiteId = _siteId, Id = userId, FirstName = "Fred", LastName = "Flintstone", Email = "duh", Phone = "123" } ;

            _userRepo.Setup( r=> r.Get(It.Is<SiteObjectIdentifier>( p=> p.SiteId == _siteId && p.Id == userId))).ReturnsAsync(user);

            var result = await _userRepo.Object.Get(_siteId, userId);

            Assert.Equal(user, result);
        }
    }

    public class User
    {
        public Guid   SiteId    { get; set; }
        public Guid   Id        { get; set; }
        public string FirstName { get; set; }
        public string LastName  { get; set; }
        public string Email     { get; set; }
        public string Phone     { get; set; }
    }
}
