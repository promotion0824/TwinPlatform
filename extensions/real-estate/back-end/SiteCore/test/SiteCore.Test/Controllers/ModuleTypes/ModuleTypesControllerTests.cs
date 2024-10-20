using AutoFixture;
using Moq;
using SiteCore.Controllers;
using SiteCore.Domain;
using SiteCore.Services;
using System;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using System.Linq;
using FluentAssertions;
using SiteCore.Dto;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Willow.ExceptionHandling.Exceptions;


namespace SiteCore.Test.Controllers.ModuleTypes
{
    public class ModuleGroupsControllerTests : BaseInMemoryTest
    {
        private readonly ModuleTypesController _controller;
        private readonly Mock<IModuleTypesService> _moduleTypesServiceMock;

        public ModuleGroupsControllerTests(ITestOutputHelper output) : base(output)
        {
            _moduleTypesServiceMock = new Mock<IModuleTypesService>();
            _controller = new ModuleTypesController(_moduleTypesServiceMock.Object);
        }

        [Fact]
        public async Task GetModuleTypes_ReturnsAllModuleTypes()
        {
            var moduleTypes = Fixture.CreateMany<ModuleType>(10).ToList();

            _moduleTypesServiceMock.Setup(x => x.GetModuleTypesAsync(It.IsAny<Guid?>())).ReturnsAsync(moduleTypes);

            var result = await _controller.GetModuleTypes();

            var moduleTypesResult = result.Result as ObjectResult;

            moduleTypesResult.StatusCode.Should().Be((int)HttpStatusCode.OK);
            moduleTypesResult.Value.Should().BeEquivalentTo(ModuleTypeDto.MapFrom(moduleTypes));
        }

        [Fact]
        public async Task GetSiteModuleTypes_ReturnsSitesModuleTypes()
        {
            var siteId = Guid.NewGuid();
            var expectedModuleTypes = Fixture.CreateMany<ModuleType>(10).ToList();

            _moduleTypesServiceMock.Setup(x => x.GetModuleTypesAsync(It.IsAny<Guid?>())).ReturnsAsync(expectedModuleTypes);

            var result = await _controller.GetSiteModuleTypes(siteId);

            var moduleTypesResult = result.Result as ObjectResult;

            moduleTypesResult.StatusCode.Should().Be((int)HttpStatusCode.OK);
            moduleTypesResult.Value.Should().BeEquivalentTo(ModuleTypeDto.MapFrom(expectedModuleTypes));
        }

        [Fact]
        public async Task CreateDefaults_ReturnsCreatedDefaultModuleTypes()
        {
            var siteId = Guid.NewGuid();
            var expectedModuleTypes = Fixture.CreateMany<ModuleType>(10).ToList();

            _moduleTypesServiceMock.Setup(x => x.CreateDefaultModuleTypesAsync(It.IsAny<Guid>())).ReturnsAsync(expectedModuleTypes);

            var result = await _controller.CreateDefaults(siteId);

            var moduleTypesResult = result.Result as ObjectResult;

            moduleTypesResult.StatusCode.Should().Be((int)HttpStatusCode.OK);
            moduleTypesResult.Value.Should().BeEquivalentTo(ModuleTypeDto.MapFrom(expectedModuleTypes));
        }

        [Fact]
        public async Task CreateModuleType_WithDuplicatedPrefix_ThrowsBadRequestException()
        {
            var siteId = Guid.NewGuid();

            _moduleTypesServiceMock.Setup(x => x.IsValidPrefix(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<Guid?>())).ReturnsAsync(false);

            await Assert.ThrowsAsync<ArgumentException>(() => _controller.CreateModuleType(siteId, new Requests.ModuleTypeRequest()));
        }

        [Fact]
        public async Task UpdateModuleType_WithInvalidId_ThrowsResourceNotFoundException()
        {
            var siteId = Guid.NewGuid();
            var id = Guid.NewGuid();
            ModuleType result = null;

            _moduleTypesServiceMock.Setup(x => x.GetModuleTypeAsync(It.IsAny<Guid>())).ReturnsAsync(result);

            await Assert.ThrowsAsync<NotFoundException>(() => _controller.UpdateModuleType(siteId, id, new Requests.ModuleTypeRequest()));
        }

        [Fact]
        public async Task UpdateModuleType_WithDuplicatedPrefix_ThrowsBadRequestException()
        {
            var siteId = Guid.NewGuid();
            var id = Guid.NewGuid();

            _moduleTypesServiceMock.Setup(x => x.GetModuleTypeAsync(It.IsAny<Guid>())).ReturnsAsync(Fixture.Create<ModuleType>());
            _moduleTypesServiceMock.Setup(x => x.IsValidPrefix(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<Guid?>())).ReturnsAsync(false);

            await Assert.ThrowsAsync<ArgumentException>(() => _controller.UpdateModuleType(siteId, id, new Requests.ModuleTypeRequest()));
        }

        [Fact]
        public async Task UpdateModuleType_ReturnsValidResult()
        {
            var siteId = Guid.NewGuid();
            var id = Guid.NewGuid();
            var entity = Fixture.Create<ModuleType>();

            _moduleTypesServiceMock.Setup(x => x.GetModuleTypeAsync(It.IsAny<Guid>())).ReturnsAsync(Fixture.Create<ModuleType>());
            _moduleTypesServiceMock.Setup(x => x.IsValidPrefix(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<Guid?>())).ReturnsAsync(true);
            _moduleTypesServiceMock.Setup(x => x.UpdateModuleTypeAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<ModuleType>())).ReturnsAsync(entity);

            var result = await _controller.UpdateModuleType(siteId, id, new Requests.ModuleTypeRequest());
            var moduleTypesResult = result.Result as ObjectResult;

            moduleTypesResult.StatusCode.Should().Be((int)HttpStatusCode.OK);
            moduleTypesResult.Value.Should().BeEquivalentTo(ModuleTypeDto.MapFrom(entity));
        }

        [Fact]
        public async Task CreateModuleType_ReturnsValidResult()
        {
            var siteId = Guid.NewGuid();
            var id = Guid.NewGuid();
            var entity = Fixture.Create<ModuleType>();

            _moduleTypesServiceMock.Setup(x => x.GetModuleTypeAsync(It.IsAny<Guid>())).ReturnsAsync(Fixture.Create<ModuleType>());
            _moduleTypesServiceMock.Setup(x => x.IsValidPrefix(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<Guid?>())).ReturnsAsync(true);
            _moduleTypesServiceMock.Setup(x => x.CreateModuleTypeAsync(It.IsAny<Guid>(), It.IsAny<ModuleType>())).ReturnsAsync(entity);

            var result = await _controller.CreateModuleType(siteId, new Requests.ModuleTypeRequest());
            var moduleTypesResult = result.Result as ObjectResult;

            moduleTypesResult.StatusCode.Should().Be((int)HttpStatusCode.OK);
            moduleTypesResult.Value.Should().BeEquivalentTo(ModuleTypeDto.MapFrom(entity));
        }

        [Fact]
        public async Task Delete_WithInvalidId_ThrowsResourceNotFoundException()
        {
            var siteId = Guid.NewGuid();
            var id = Guid.NewGuid();
            ModuleType result = null;

            _moduleTypesServiceMock.Setup(x => x.GetModuleTypeAsync(It.IsAny<Guid>())).ReturnsAsync(result);

            await Assert.ThrowsAsync<NotFoundException>(() => _controller.Delete(id));
        }

        [Fact]
        public async Task Delete_WithModuleTypeWithAssignments_ThrowsBadRequestException()
        {
            var siteId = Guid.NewGuid();
            var id = Guid.NewGuid();

            _moduleTypesServiceMock.Setup(x => x.GetModuleTypeAsync(It.IsAny<Guid>())).ReturnsAsync(new ModuleType { HasModuleAssignments = true });

            await Assert.ThrowsAsync<ArgumentException>(() => _controller.Delete(id));
        }
    }
}
