using AutoFixture;
using Azure.DigitalTwins.Core;
using Moq;
using System.Threading.Tasks;
using Willow.AzureDigitalTwins.Api.Controllers;
using Willow.AzureDigitalTwins.Api.Services;
using Willow.AzureDigitalTwins.Services.Extensions;
using Willow.Model.Adt;
using Willow.Model.Requests;
using Xunit;

namespace Willow.AzureDigitalTwins.Api.UnitTests.Controllers
{
    public class QueryControllerTests
    {
        private readonly QueryController _queryController;
        private readonly Mock<ITwinsService> _twinsServiceMock;
        private readonly Fixture _fixture;

        public QueryControllerTests()
        {
            _twinsServiceMock = new Mock<ITwinsService>();
            _queryController = new QueryController(_twinsServiceMock.Object);
            _fixture = new Fixture();
        }


        [Fact]
        public async Task QueryTwinsWithRelationships_ShouldReturnTwins()
        {
            var twins = _fixture.CreateMany<TwinWithRelationships>(10).ToPageModel(1, 500);

            _twinsServiceMock.Setup(x => x.QueryTwinsAsync(It.IsAny<QueryTwinsRequest>(), It.IsAny<SourceType>(), It.IsAny<int>(), null))
                .ReturnsAsync(twins);
            _twinsServiceMock.Setup(x => x.AppendRelationships(It.IsAny<Page<BasicDigitalTwin>>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(new Page<TwinWithRelationships>());

            var response = await _queryController.QueryTwinsWithRelationships(new QueryTwinsRequest { Query = "DUMMY QUERY" });

            Assert.NotNull(response);
            Assert.NotNull(response.Value);
        }
    }
}
