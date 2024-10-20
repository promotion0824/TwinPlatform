using AutoFixture;
using DigitalTwinCore.Entities;
using DigitalTwinCore.Models;
using DigitalTwinCore.Services;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using Willow.Tests.Infrastructure;
using Workflow.Tests;
using Xunit;
using Xunit.Abstractions;

namespace DigitalTwinCore.Test.Services.TagMapperService
{
    public class MapTagsTests : BaseInMemoryTest
    {
        public MapTagsTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void NoVirtualTagsExist_MapTags_ReturnsOriginalList()
        {
            var siteId = Guid.NewGuid();
            using var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb);

            var serverArrangement = server.Arrange();
            var tagMapperService = serverArrangement.MainServices.GetRequiredService<ITagMapperService>();

            var expectedTags = CreateTagList();
            var output = tagMapperService.MapTags(siteId, Fixture.Create<string>(), ConvertTagsToDictionary(expectedTags));

            output.Should().BeEquivalentTo(expectedTags);
        }

        [Fact]
        public void VirtualTagsExistButTagListDoesNotMatch_MapTags_ReturnsOriginalList()
        {
            var otherSiteId = Guid.NewGuid();
            var siteId = Guid.NewGuid();
            var virtualTagName = Fixture.Create<string>();

            using var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb);
            var serverArrangement = server.Arrange();
            var tagMapperService = serverArrangement.MainServices.GetRequiredService<ITagMapperService>();

            var expectedTags = CreateTagList();
            var virtualTag = CreateVirtualTagInDb(serverArrangement, otherSiteId, virtualTagName, null, CreateTagList().Select(t => t.Name));
            var output = tagMapperService.MapTags(siteId, Fixture.Create<string>(), ConvertTagsToDictionary(expectedTags));

            output.Should().BeEquivalentTo(expectedTags);
        }

        [Fact]
        public void ModelIdMatchesVirtualTag_MapTags_ReturnsVirtualTagInList()
        {
            var siteId = Guid.NewGuid();
            var virtualTagName = Fixture.Create<string>();
            var modelId = Fixture.Create<string>();

            using var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb);
            var serverArrangement = server.Arrange();

            var tagMapperService = serverArrangement.MainServices.GetRequiredService<ITagMapperService>();
            var virtualTag = CreateVirtualTagInDb(serverArrangement, siteId, virtualTagName, modelId, null);
            var expectedTags = CreateTagList();

            var output = tagMapperService.MapTags(siteId, modelId, ConvertTagsToDictionary(expectedTags));

            output.Should().BeEquivalentTo(AddVirtualTagToExpectedTags(expectedTags, virtualTag));
        }

        [Fact]
        public void TagListMatchesVirtualTag_MapTags_ReturnsVirtualTagInList()
        {
            var siteId = Guid.NewGuid();
            var virtualTagName = Fixture.Create<string>();

            using var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb);
            var serverArrangement = server.Arrange();

            var tagMapperService = serverArrangement.MainServices.GetRequiredService<ITagMapperService>();
            var expectedTags = CreateTagList();
            var virtualTag = CreateVirtualTagInDb(serverArrangement, siteId, virtualTagName, null, expectedTags.Select(t => t.Name) );

            var output = tagMapperService.MapTags(siteId, Fixture.Create<string>(), ConvertTagsToDictionary(expectedTags));

            output.Should().BeEquivalentTo(AddVirtualTagToExpectedTags(expectedTags, virtualTag));
        }

        [Fact]
        public void TagListMatchesVirtualTagOfDifferentSite_MapTags_ReturnsOriginalList()
        {
            var otherSiteId = Guid.NewGuid();
            var siteId = Guid.NewGuid();
            var virtualTagName = Fixture.Create<string>();

            using var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb);
            var serverArrangement = server.Arrange();
            var tagMapperService = serverArrangement.MainServices.GetRequiredService<ITagMapperService>();

            var expectedTags = CreateTagList();
            var virtualTag = CreateVirtualTagInDb(serverArrangement, otherSiteId, virtualTagName, null, expectedTags.Select(t => t.Name));
            var output = tagMapperService.MapTags(siteId, Fixture.Create<string>(), ConvertTagsToDictionary(expectedTags));

            output.Should().BeEquivalentTo(expectedTags);
        }

        [Fact]
        public void TagListIsSuperSetOfVirtualTag_MapTags_ReturnsOriginalList()
        {
            var siteId = Guid.NewGuid();
            var virtualTagName = Fixture.Create<string>();

            var modelId = Fixture.Create<string>();
            using var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb);
            var serverArrangement = server.Arrange();
            var tagMapperService = serverArrangement.MainServices.GetRequiredService<ITagMapperService>();

            var virtualTagList = CreateTagList();
            var virtualTag = CreateVirtualTagInDb(serverArrangement, siteId, virtualTagName, null, virtualTagList.Select(t => t.Name));

            var expectedTags = virtualTagList.Concat(CreateTagList()).ToList();
            var output = tagMapperService.MapTags(siteId, Fixture.Create<string>(), ConvertTagsToDictionary(expectedTags));

            output.Should().BeEquivalentTo(expectedTags);
        }

        [Fact]
        public void ModelIdMatchesVirtualTagButVirtualTagAlreadyInList_MapTags_ReturnsOriginalListWithFeatureSet()
        {
            var siteId = Guid.NewGuid();
            var modelId = Fixture.Create<string>();

            using var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb);
            var serverArrangement = server.Arrange();
            var tagMapperService = serverArrangement.MainServices.GetRequiredService<ITagMapperService>();

            var expectedTags = CreateTagList();
            var virtualTagName = expectedTags.Last().Name;
            var virtualTag = CreateVirtualTagInDb(serverArrangement, siteId, virtualTagName, null, expectedTags.Select(t => t.Name));

            var output = tagMapperService.MapTags(siteId, modelId, ConvertTagsToDictionary(expectedTags));
            expectedTags.Last().Type = TagType.TwoD;

            output.Should().BeEquivalentTo(expectedTags);
        }

        [Fact]
        public void TagListMatchesVirtualTagButVirtualTagAlreadyInList_MapTags_ReturnsOriginalListWithFeatureSet()
        {
            var siteId = Guid.NewGuid();
            var modelId = Fixture.Create<string>();

            using var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb);
            var serverArrangement = server.Arrange();
            var tagMapperService = serverArrangement.MainServices.GetRequiredService<ITagMapperService>();

            var expectedTags = CreateTagList();
            var virtualTagName = expectedTags.Last().Name;
            var virtualTag = CreateVirtualTagInDb(serverArrangement, siteId, virtualTagName, null, expectedTags.Select(t => t.Name));

            var output = tagMapperService.MapTags(siteId, modelId, ConvertTagsToDictionary(expectedTags));
            expectedTags.Last().Type = TagType.TwoD;

            output.Should().BeEquivalentTo(expectedTags);
        }

        private List<Tag> CreateTagList() =>
            Fixture.Build<Tag>().With(t => t.Type, TagType.General).CreateMany(3).OrderBy(t => t.Name).ToList();

        private static Dictionary<string, object> ConvertTagsToDictionary(List<Tag> expectedTags) =>
            new Dictionary<string, object>(expectedTags.Select(t => new KeyValuePair<string, object>(t.Name, true)));

        private static List<Tag> AddVirtualTagToExpectedTags(List<Tag> expectedTags, Tag virtualTag) =>
            expectedTags.Append(virtualTag).OrderBy(t => t.Name).ToList();

        private static Tag CreateVirtualTagInDb(ServerArrangement serverArrangement, Guid siteId, string virtualTagName, string modelId, IEnumerable<string> matchTagList)
        {
            var context = serverArrangement.CreateDbContext<DigitalTwinDbContext>();
            var entity = new TagEntity { Name = virtualTagName, TagType = (int)TagType.TwoD };
            context.Tags.Add(entity);
            context.VirtualTags.Add(new SiteVirtualTagEntity
            {
                Id = Guid.NewGuid(),
                SiteId = siteId,
                Tag = virtualTagName,
                MatchModelId = modelId,
                MatchTagList = matchTagList == null ? null : string.Join(',', matchTagList.OrderBy(t => t))
            });

            context.SaveChanges();

            return new Tag { Name = entity.Name, Type = (TagType)entity.TagType };
        }
    }
}
