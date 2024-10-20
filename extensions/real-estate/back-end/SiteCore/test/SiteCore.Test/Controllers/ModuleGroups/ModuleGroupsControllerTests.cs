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
using FluentAssertions;
using SiteCore.Dto;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Willow.ExceptionHandling.Exceptions;

namespace SiteCore.Test.Controllers.ModuleGroups
{
    public class ModuleGroupsControllerTests : BaseInMemoryTest
    {
        private readonly ModuleGroupsController _controller;
        private readonly Mock<IModuleGroupsService> _moduleGroupsServiceMock;

        public ModuleGroupsControllerTests(ITestOutputHelper output) : base(output)
        {
            _moduleGroupsServiceMock = new Mock<IModuleGroupsService>();
            _controller = new ModuleGroupsController(_moduleGroupsServiceMock.Object);
        }

        [Fact]
        public async Task UpdateModuleGroup_WithInvalidId_ThrowsResourceNotFoundException()
        {
            var siteId = Guid.NewGuid();
            var id = Guid.NewGuid();
            ModuleGroup result = null;

            _moduleGroupsServiceMock.Setup(x => x.GetModuleGroupAsync(It.IsAny<Guid>())).ReturnsAsync(result);

            await Assert.ThrowsAsync<NotFoundException>(() => _controller.UpdateModuleGroup(siteId, id, new Requests.ModuleGroupRequest()));
        }

        [Fact]
        public async Task UpdateModuleGroup_WithDuplicatedPrefix_ThrowsBadRequestException()
        {
            var siteId = Guid.NewGuid();
            var id = Guid.NewGuid();

            _moduleGroupsServiceMock.Setup(x => x.GetModuleGroupAsync(It.IsAny<Guid>())).ReturnsAsync(Fixture.Create<ModuleGroup>());
            _moduleGroupsServiceMock.Setup(x => x.GetModuleGroupByNameAsync(It.IsAny<Guid>(), It.IsAny<string>())).ReturnsAsync(Fixture.Create<ModuleGroup>());

            await Assert.ThrowsAsync<ArgumentException>(() => _controller.UpdateModuleGroup(siteId, id, new Requests.ModuleGroupRequest()));
        }

        [Fact]
        public async Task UpdateModuleGroup_ReturnsValidResult()
        {
            var siteId = Guid.NewGuid();
            var id = Guid.NewGuid();
            var entity = Fixture.Build<ModuleGroup>().With(x => x.Id, id).With(x => x.SiteId, siteId).Create();

            _moduleGroupsServiceMock.Setup(x => x.GetModuleGroupAsync(It.IsAny<Guid>())).ReturnsAsync(entity);
            _moduleGroupsServiceMock.Setup(x => x.GetModuleGroupByNameAsync(It.IsAny<Guid>(), It.IsAny<string>())).ReturnsAsync(entity);
            _moduleGroupsServiceMock.Setup(x => x.UpdateModuleGroupAsync(It.IsAny<ModuleGroup>())).ReturnsAsync(entity);

            var result = await _controller.UpdateModuleGroup(siteId, id, new Requests.ModuleGroupRequest());
            var moduleTypesResult = result.Result as ObjectResult;

            moduleTypesResult.StatusCode.Should().Be((int)HttpStatusCode.OK);
            Assert.Equal(((ModuleGroupDto)moduleTypesResult.Value).Id, entity.Id);
        }
    }
}
