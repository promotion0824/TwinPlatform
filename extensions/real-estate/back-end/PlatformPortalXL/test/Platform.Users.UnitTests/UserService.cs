using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Moq;

using Willow.Api.Client;
using Willow.Data;
using Willow.Platform.Users;

namespace Willow.Platform.Users.UnitTests
{
    public class UserServiceTests
    {
        private readonly IUserService _service;
        private readonly Mock<IRestApi> _directoryApi = new Mock<IRestApi>();
        private readonly Mock<IReadRepository<Guid, User>> _userRepo  = new Mock<IReadRepository<Guid, User>>();
        private readonly Mock<IReadRepository<SiteObjectIdentifier, Workgroup>> _workgroupRepo  = new Mock<IReadRepository<SiteObjectIdentifier, Workgroup>>();
        private readonly Guid _siteId = Guid.NewGuid();

        public UserServiceTests()
        {
            _service = new UserService(_userRepo.Object, _workgroupRepo.Object, _directoryApi.Object);
        }

        [Fact]
        public async Task UserService_GetCustomerUser_success()
        {
            _directoryApi.Setup( d=> d.Get<User>(It.IsAny<string>(), null)).ReturnsAsync(new User { FirstName = "Bob" } );

            var result = await _service.GetCustomerUser(Guid.NewGuid(), Guid.NewGuid());

            Assert.NotNull(result);
            Assert.Equal("Bob", result.FirstName);
        }

        [Fact]
        public async Task UserService_GetUser_success()
        {
            var idUser1 = Guid.NewGuid();

            _userRepo.Setup( d=> d.Get(idUser1)).ReturnsAsync(new User { Id = idUser1, FirstName = "Fred" } );
            _userRepo.Setup( d=> d.Get(It.IsAny<IEnumerable<Guid>>())).Returns(GetAsyncList(new User { Id = idUser1, FirstName = "Fred" }));

            var result = await _service.GetUser(Guid.NewGuid(), idUser1);

            Assert.NotNull(result);
            Assert.Equal("Fred", result.Name);
        }


        [Fact]
        public async Task UserService_GetUser_workgroup_success()
        {
            var idUser1 = Guid.NewGuid();

            _workgroupRepo.Setup( d=> d.Get(It.IsAny<IEnumerable<SiteObjectIdentifier>>())).Returns(GetAsyncList(new Workgroup { Id = idUser1, Name = "Fred" } ));
            _userRepo.Setup( d=> d.Get(It.IsAny<IEnumerable<Guid>>())).Returns(GetAsyncListFromList(new List<User> {}));
            var result = await _service.GetUser(_siteId, idUser1);

            Assert.NotNull(result);
            Assert.Equal("Fred", result.Name);
        }

        [Fact]
        public async Task UserService_GetUsers_types_success()
        {
            var idUser1 = Guid.NewGuid();
            var idUser2 = Guid.NewGuid();
            var idUser3 = Guid.NewGuid();
            var idUser4 = Guid.NewGuid();
            var idUser5 = Guid.NewGuid();
            var idUser6 = Guid.NewGuid();
            var idUser7 = Guid.NewGuid();
            var idUser8 = Guid.NewGuid();
            var idUser9 = Guid.NewGuid();
            var idUser10 = Guid.NewGuid();

            _userRepo.Setup( d=> d.Get(idUser9)).ThrowsAsync( new RestNotFoundException() );

            var users = new List<User>
            {
                new User { Id = idUser2, FirstName = "Fred" },
                new User { Id = idUser3, FirstName = "Wilma" },
                new User { Id = idUser6, FirstName = "Dino" },
                new User { Id = idUser7, FirstName = "Pebbles" }
            };

            _userRepo.Setup( d=> d.Get(It.IsAny<IEnumerable<Guid>>())).Returns(GetAsyncListFromList(users));

            _workgroupRepo.Setup( d=> d.Get(It.IsAny<IEnumerable<SiteObjectIdentifier>>())).Returns(GetAsyncList(new Workgroup { Name = "Fun Group" } ));

            var ids = new List<Guid> { idUser2, idUser3, idUser6, idUser8, idUser7, idUser9, idUser10 };

            var result = await _service.GetUsers(_siteId, ids.Select( id=> (id, UserType.Unknown)), UserType.Customer | UserType.Workgroup );

            Assert.NotNull(result);
            Assert.Equal(5, result.Count);
            Assert.True(result.Where( u=> u.Name == "Fred").Any());
            Assert.True(result.Where( u=> u.Name == "Wilma").Any());
            Assert.True(result.Where( u=> u.Name == "Dino").Any());
            Assert.True(result.Where( u=> u.Name == "Pebbles").Any());
            Assert.True(result.Where( u=> u.Name == "Fun Group").Any());
        }

        [Theory]
        [InlineData(UserType.Customer | UserType.Workgroup, 5)]
        [InlineData(UserType.All, 5)]
        public async Task UserService_GetUsers_partial_success(UserType types, int expectedCount)
        {
            var idUser1 = Guid.NewGuid();
            var idUser2 = Guid.NewGuid();
            var idUser3 = Guid.NewGuid();
            var idUser4 = Guid.NewGuid();
            var idUser5 = Guid.NewGuid();
            var idUser6 = Guid.NewGuid();
            var idUser7 = Guid.NewGuid();
            var idUser8 = Guid.NewGuid();
            var idUser9 = Guid.NewGuid();
            var idUser10 = Guid.NewGuid();

            _userRepo.Setup( d=> d.Get(idUser9)).ThrowsAsync( new RestNotFoundException() );

            var users = new List<User>
            {
                new User { Id = idUser2, FirstName = "Fred" },
                new User { Id = idUser3, FirstName = "Wilma" },
                new User { Id = idUser6, FirstName = "Dino" },
                new User { Id = idUser7, FirstName = "Pebbles" }
            };

            _userRepo.Setup( d=> d.Get(It.IsAny<IEnumerable<Guid>>())).Returns(GetAsyncListFromList(users));

            _workgroupRepo.Setup( d=> d.Get(It.Is<IEnumerable<SiteObjectIdentifier>>( list=> list.Any(i=> i.SiteId == _siteId && i.Id == idUser1)))).Returns(GetAsyncList(new Workgroup { Id = idUser1, Name = "Fun Group" } ));
           
            var ids = new List<Guid> { idUser1, idUser2, idUser3, idUser4, idUser6, idUser8, idUser7, idUser9, idUser10, Guid.NewGuid() };

            var result = await _service.GetUsers(_siteId, ids.Select( id=> (id, UserType.Unknown)), types);

            Assert.NotNull(result);
            Assert.Equal(expectedCount, result.Count);

            if((types & UserType.Customer) == UserType.Customer)
            {
                Assert.True(result.Where( u=> u.Name == "Fred").Any());
                Assert.True(result.Where( u=> u.Name == "Wilma").Any());
                Assert.True(result.Where( u=> u.Name == "Dino").Any());
                Assert.True(result.Where( u=> u.Name == "Pebbles").Any());
            }

            if((types & UserType.Customer) == UserType.Workgroup)
            {
                Assert.True(result.Where( u=> u.Name == "Fun Group").Any());
            }
        }

        private async IAsyncEnumerable<T> GetAsyncList<T>(T item)
        {
            yield return await Task.FromResult(item);
        }

        private async IAsyncEnumerable<T> GetAsyncListFromList<T>(IEnumerable<T> items)
        {
            foreach(var item in items)
                yield return await Task.FromResult(item);
        }
     }
}
