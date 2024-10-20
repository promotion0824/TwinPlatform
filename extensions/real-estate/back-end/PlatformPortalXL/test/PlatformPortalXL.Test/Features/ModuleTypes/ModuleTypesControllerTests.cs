using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Features.SiteStructure;
using PlatformPortalXL.Features.SiteStructure.Requests;
using PlatformPortalXL.Helpers;
using PlatformPortalXL.Models;
using PlatformPortalXL.Services;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Willow.Common;
using Willow.Directory.Models;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.ModuleTypes
{
    public class ModuleTypesControllerTests : BaseInMemoryTest
    {
        private ModuleTypesController moduleTypesController;
        private Mock<IAccessControlService> accessControlServiceMock;
        private Mock<IModuleTypesService> moduleTypesServiceMock;
        private Mock<IControllerHelper> controllerHelperMock;

        public ModuleTypesControllerTests(ITestOutputHelper output) : base(output)
        {
            accessControlServiceMock = new Mock<IAccessControlService>();
            moduleTypesServiceMock = new Mock<IModuleTypesService>();
            controllerHelperMock = new Mock<IControllerHelper>();
            moduleTypesController = new ModuleTypesController(accessControlServiceMock.Object, moduleTypesServiceMock.Object, controllerHelperMock.Object);
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_CreateDefaultModuleTypes_ThorwUnauthorizedAccessExceptio()
        {
            await ValidatePermissionsTest(() => moduleTypesController.CreateDefaultModuleTypes(Guid.NewGuid()));
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_CreateModuleType_ThorwsUnauthorizedAccessExceptio()
        {
            await ValidatePermissionsTest(() => moduleTypesController.CreateModuleType(Guid.NewGuid(), new ModuleTypeRequest()));
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_DeleteModuleType_ThorwsUnauthorizedAccessExceptio()
        {
            await ValidatePermissionsTest(() => moduleTypesController.DeleteModuleType(Guid.NewGuid(), Guid.NewGuid()));
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_GetSiteModuleTypes_ThorwsUnauthorizedAccessExceptio()
        {
            await ValidatePermissionsTest(() => moduleTypesController.GetSiteModuleTypes(Guid.NewGuid()));
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_UpdateModuleType_ThorwsUnauthorizedAccessExceptio()
        {
            await ValidatePermissionsTest(() => moduleTypesController.UpdateModuleType(Guid.NewGuid(), Guid.NewGuid(), new ModuleTypeRequest()));
        }

        private async Task ValidatePermissionsTest(Func<Task> action)
        {
            controllerHelperMock.Setup(x => x.GetCurrentUserId(It.IsAny<ControllerBase>())).Returns(Guid.NewGuid());
            accessControlServiceMock.Setup(x => x.EnsureAccessSite(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid>()))
                .Throws(new UnauthorizedAccessException().WithData(new { userId = Guid.NewGuid(), string.Empty, RoleResourceType.Site, resourceId = Guid.NewGuid() }));

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => action());
        }

        [Fact]
        public async Task GetModuleTypesAsync_ReturnsSiteModuleTypes()
        {
            var siteId = Guid.NewGuid();
            var moduleTypes = Fixture.CreateMany<ModuleType>(10).ToList();

            moduleTypesServiceMock.Setup(x => x.GetModuleTypesAsync(It.IsAny<Guid>())).ReturnsAsync(moduleTypes);

            var response = await moduleTypesController.GetSiteModuleTypes(siteId);
            var result = response.Result as OkObjectResult;

            result.StatusCode.Should().Be((int)HttpStatusCode.OK);
            result.Value.Should().BeEquivalentTo(ModuleTypeDto.MapFrom(moduleTypes));
        }

        [Fact]
        public async Task CreateDefaultModuleTypesAsync_ReturnsCreatedSiteModuleTypes()
        {
            var siteId = Guid.NewGuid();
            var moduleTypes = Fixture.CreateMany<ModuleType>(10).ToList();

            moduleTypesServiceMock.Setup(x => x.CreateDefaultModuleTypesAsync(It.IsAny<Guid>())).ReturnsAsync(moduleTypes);

            var response = await moduleTypesController.CreateDefaultModuleTypes(siteId);
            var result = response.Result as OkObjectResult;

            result.StatusCode.Should().Be((int)HttpStatusCode.OK);
            result.Value.Should().BeEquivalentTo(ModuleTypeDto.MapFrom(moduleTypes));
        }

        [Fact]
        public async Task CreateModuleTypeAsync_ReturnsCreatedSiteModuleType()
        {
            var siteId = Guid.NewGuid();
            var moduleType = Fixture.Create<ModuleType>();

            moduleTypesServiceMock.Setup(x => x.CreateModuleTypeAsync(It.IsAny<Guid>(), It.IsAny<ModuleTypeRequest>())).ReturnsAsync(moduleType);

            var response = await moduleTypesController.CreateModuleType(siteId, Fixture.Create<ModuleTypeRequest>());
            var result = response.Result as OkObjectResult;

            result.StatusCode.Should().Be((int)HttpStatusCode.OK);
            result.Value.Should().BeEquivalentTo(ModuleTypeDto.MapFrom(moduleType));
        }

        [Fact]
        public async Task UpdateModuleType_ReturnsUpdatedSiteModuleType()
        {
            var siteId = Guid.NewGuid();
            var moduleType = Fixture.Create<ModuleType>();

            moduleTypesServiceMock.Setup(x => x.UpdateModuleTypeAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<ModuleTypeRequest>())).ReturnsAsync(moduleType);

            var response = await moduleTypesController.UpdateModuleType(siteId, Guid.NewGuid(), Fixture.Create<ModuleTypeRequest>());
            var result = response.Result as OkObjectResult;

            result.StatusCode.Should().Be((int)HttpStatusCode.OK);
            result.Value.Should().BeEquivalentTo(ModuleTypeDto.MapFrom(moduleType));
        }

        [Fact]
        public async Task DeleteModuleType_ReturnsOk()
        {
            var siteId = Guid.NewGuid();
            var moduleType = Fixture.Create<ModuleType>();

            moduleTypesServiceMock.Setup(x => x.DeleteModuleTypeAsync(It.IsAny<Guid>(), It.IsAny<Guid>()));

            var response = await moduleTypesController.DeleteModuleType(siteId, Guid.NewGuid());
            var result = response as OkResult;

            result.StatusCode.Should().Be((int)HttpStatusCode.OK);
        }
    }
}
