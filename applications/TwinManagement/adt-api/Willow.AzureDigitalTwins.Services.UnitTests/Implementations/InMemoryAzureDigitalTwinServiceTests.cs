namespace Willow.AzureDigitalTwins.Services.UnitTests.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoFixture;
    using Azure;
    using Azure.DigitalTwins.Core;
    using Moq;
    using Willow.AzureDigitalTwins.Services.UnitTests.Fixtures;
    using Willow.Model.Requests;
    using Xunit;

    public class InMemoryAzureDigitalTwinServiceTests : IClassFixture<AzureDigitalTwinServiceFixture>
    {
        private readonly AzureDigitalTwinServiceFixture azureDigitalTwinServiceFixture;
        private readonly Fixture fixture;

        public InMemoryAzureDigitalTwinServiceTests(AzureDigitalTwinServiceFixture azureDigitalTwinServiceFixture)
        {
            this.azureDigitalTwinServiceFixture = azureDigitalTwinServiceFixture;
            fixture = new Fixture();
        }

        private static int GetRandomIndex(int count)
        {
            return new Random().Next(1, count - 1);
        }

        [Theory]
        [InlineData(false, false, 0)]
        [InlineData(true, true, 2)]
        [InlineData(false, true, 1)]
        [InlineData(true, false, 1)]
        public async void AppendRelationships_azureDigitalTwinServiceFixture_TwinWithRelationship(bool includeRelationships, bool includeIncomingRelationships, int expectedRelationshipsCount)
        {
            var twinsWithRelationships = await azureDigitalTwinServiceFixture.InMemoryAzureDigitalTwinReader.AppendRelationships(new List<BasicDigitalTwin> { azureDigitalTwinServiceFixture.TwinWithRelationship }, includeRelationships, includeIncomingRelationships);

            Assert.Single(twinsWithRelationships);

            var relationships = twinsWithRelationships.First();

            Assert.Equal(expectedRelationshipsCount, relationships.Item2.Count() + relationships.Item3.Count());
        }

        [Fact]
        public async void GetDigitalTwinAsync_WithValidTwinId_ShouldReturnTwin()
        {
            var twin = azureDigitalTwinServiceFixture.Twins[GetRandomIndex(azureDigitalTwinServiceFixture.Twins.Count)];

            var serviceTwin = await azureDigitalTwinServiceFixture.InMemoryAzureDigitalTwinReader.GetDigitalTwinAsync(twin.Id);

            Assert.Equal(twin.Id, serviceTwin.Id);
        }

        [Fact]
        public async void GetDigitalTwinAsync_WithInValidTwinId_ShouldReturnNull()
        {
            var serviceTwin = await azureDigitalTwinServiceFixture.InMemoryAzureDigitalTwinReader.GetDigitalTwinAsync(Guid.NewGuid().ToString());

            Assert.Null(serviceTwin);
        }

        [Fact]
        public async virtual void CreateOrReplaceDigitalTwinAsync_ShouldCreate()
        {
            var twin = fixture.Create<BasicDigitalTwin>();

            azureDigitalTwinServiceFixture.Twins.Add(twin);

            var responseTwin = await azureDigitalTwinServiceFixture.InMemoryAzureDigitalTwinWriter.CreateOrReplaceDigitalTwinAsync(twin);
            var currentTwinsCount = await azureDigitalTwinServiceFixture.InMemoryAzureDigitalTwinReader.GetTwinsCountAsync();

            Assert.Equal(azureDigitalTwinServiceFixture.Twins.Count, currentTwinsCount);
            Assert.Equal(twin.Id, responseTwin.Id);
        }

        [Fact]
        public async void CreateOrReplaceDigitalTwinAsync_WithExisting_ShouldReplace()
        {
            var twin = azureDigitalTwinServiceFixture.Twins[GetRandomIndex(azureDigitalTwinServiceFixture.Twins.Count)];

            var responseTwin = await azureDigitalTwinServiceFixture.InMemoryAzureDigitalTwinWriter.CreateOrReplaceDigitalTwinAsync(twin);
            var currentTwinsCount = await azureDigitalTwinServiceFixture.InMemoryAzureDigitalTwinReader.GetTwinsCountAsync();

            Assert.Equal(azureDigitalTwinServiceFixture.Twins.Count, currentTwinsCount);
            Assert.Equal(twin.Id, responseTwin.Id);
        }

        [Fact]
        public async virtual void UpdateDigitalTwinAsync_WithExisting_ShouldUpdate()
        {
            var twin = azureDigitalTwinServiceFixture.Twins[GetRandomIndex(azureDigitalTwinServiceFixture.Twins.Count)];

            twin.Contents.Add("UpdatedKey", "UpdatedValue");

            await azureDigitalTwinServiceFixture.InMemoryAzureDigitalTwinWriter.UpdateDigitalTwinAsync(twin, fixture.Create<JsonPatchDocument>());
            var currentTwinsCount = await azureDigitalTwinServiceFixture.InMemoryAzureDigitalTwinReader.GetTwinsCountAsync();
            var updatedTwin = await azureDigitalTwinServiceFixture.InMemoryAzureDigitalTwinReader.GetDigitalTwinAsync(twin.Id);

            Assert.Equal(azureDigitalTwinServiceFixture.Twins.Count, currentTwinsCount);
            Assert.True(updatedTwin.Contents.ContainsKey("UpdatedKey"));
            Assert.Equal("UpdatedValue", updatedTwin.Contents["UpdatedKey"]);
        }

        [Fact]
        public async virtual void CreateOrReplaceRelationshipAsync_ShouldCreate()
        {
            var relationship = fixture.Create<BasicRelationship>();

            azureDigitalTwinServiceFixture.Relationships.Add(relationship);

            var response = await azureDigitalTwinServiceFixture.InMemoryAzureDigitalTwinWriter.CreateOrReplaceRelationshipAsync(relationship);
            var currentCount = await azureDigitalTwinServiceFixture.InMemoryAzureDigitalTwinReader.GetRelationshipsCountAsync();

            Assert.Equal(azureDigitalTwinServiceFixture.Relationships.Count, currentCount);
            Assert.Equal(relationship.Id, response.Id);
        }

        [Fact]
        public async void CreateOrReplaceRelationshipAsync_WithExisting_ShouldReplace()
        {
            var relationship = azureDigitalTwinServiceFixture.Relationships[GetRandomIndex(azureDigitalTwinServiceFixture.Relationships.Count)];

            var response = await azureDigitalTwinServiceFixture.InMemoryAzureDigitalTwinWriter.CreateOrReplaceRelationshipAsync(relationship);
            var currentCount = await azureDigitalTwinServiceFixture.InMemoryAzureDigitalTwinReader.GetRelationshipsCountAsync();

            Assert.Equal(azureDigitalTwinServiceFixture.Relationships.Count, currentCount);
            Assert.Equal(relationship.Id, response.Id);
        }

        [Fact]
        public async virtual void DeleteDigitalTwinAsync_ShouldRemove()
        {
            var index = GetRandomIndex(azureDigitalTwinServiceFixture.Twins.Count);
            var twin = azureDigitalTwinServiceFixture.Twins[index];

            azureDigitalTwinServiceFixture.Twins.RemoveAt(index);

            await azureDigitalTwinServiceFixture.InMemoryAzureDigitalTwinWriter.DeleteDigitalTwinAsync(twin.Id);
            var currentCount = await azureDigitalTwinServiceFixture.InMemoryAzureDigitalTwinReader.GetTwinsCountAsync();

            Assert.Equal(azureDigitalTwinServiceFixture.Twins.Count, currentCount);
        }

        [Fact]
        public async void DeleteDigitalTwinAsync_NonExisting_ShouldNotRemove()
        {
            await azureDigitalTwinServiceFixture.InMemoryAzureDigitalTwinWriter.DeleteDigitalTwinAsync(Guid.NewGuid().ToString());
            var currentCount = await azureDigitalTwinServiceFixture.InMemoryAzureDigitalTwinReader.GetTwinsCountAsync();

            Assert.Equal(azureDigitalTwinServiceFixture.Twins.Count, currentCount);
        }

        [Fact]
        public async virtual void DeleteRelationshipAsync_ShouldRemove()
        {
            var index = GetRandomIndex(azureDigitalTwinServiceFixture.Relationships.Count);
            var relationship = azureDigitalTwinServiceFixture.Relationships[index];

            azureDigitalTwinServiceFixture.Relationships.RemoveAt(index);

            await azureDigitalTwinServiceFixture.InMemoryAzureDigitalTwinWriter.DeleteRelationshipAsync(relationship.SourceId, relationship.Id);
            var currentCount = await azureDigitalTwinServiceFixture.InMemoryAzureDigitalTwinReader.GetRelationshipsCountAsync();

            Assert.Equal(azureDigitalTwinServiceFixture.Relationships.Count, currentCount);

            await azureDigitalTwinServiceFixture.InMemoryAzureDigitalTwinWriter.CreateOrReplaceRelationshipAsync(relationship);
            azureDigitalTwinServiceFixture.Relationships.Add(relationship);
        }

        [Fact]
        public async void DeleteRelationshipAsync_NonExisting_ShouldNotRemove()
        {
            await azureDigitalTwinServiceFixture.InMemoryAzureDigitalTwinWriter.DeleteRelationshipAsync(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
            var currentCount = await azureDigitalTwinServiceFixture.InMemoryAzureDigitalTwinReader.GetRelationshipsCountAsync();

            Assert.Equal(azureDigitalTwinServiceFixture.Relationships.Count, currentCount);
        }

        [Fact]
        public async void DeleteModelAsync_NonExisting_ShouldNotRemove()
        {
            await azureDigitalTwinServiceFixture.InMemoryAzureDigitalTwinWriter.DeleteModelAsync(Guid.NewGuid().ToString());
            var currentCount = await azureDigitalTwinServiceFixture.InMemoryAzureDigitalTwinReader.GetModelsAsync();

            Assert.Equal(azureDigitalTwinServiceFixture.Models.Count, currentCount.Count());
        }

        [Fact]
        public async void GetIncomingRelationshipsAsync_ShouldReturnIncoming()
        {
            var relationships = await azureDigitalTwinServiceFixture.InMemoryAzureDigitalTwinReader.GetIncomingRelationshipsAsync(azureDigitalTwinServiceFixture.TwinWithRelationship.Id);

            Assert.Single(relationships);
        }

        [Fact]
        public async void GetIncomingRelationshipsAsync_WithNoIncoming_ShouldReturnEmpty()
        {
            var twinWithNoRelationships = azureDigitalTwinServiceFixture.Twins.First(x => x.Id != azureDigitalTwinServiceFixture.TwinWithRelationship.Id);

            var relationships = await azureDigitalTwinServiceFixture.InMemoryAzureDigitalTwinReader.GetIncomingRelationshipsAsync(twinWithNoRelationships.Id);

            Assert.Empty(relationships);
        }

        [Fact]
        public async void GetModelAsync_ShouldReturnModel()
        {
            var model = azureDigitalTwinServiceFixture.Models[GetRandomIndex(azureDigitalTwinServiceFixture.Models.Count)];

            var response = await azureDigitalTwinServiceFixture.InMemoryAzureDigitalTwinReader.GetModelAsync(model.Id);

            Assert.Equal(model.Id, response.Id);
        }

        [Fact]
        public async void GetModelAsync_NonExisting_ShouldReturnNull()
        {
            var response = await azureDigitalTwinServiceFixture.InMemoryAzureDigitalTwinReader.GetModelAsync(Guid.NewGuid().ToString());

            Assert.Null(response);
        }

        [Fact]
        public async void GetModelsAsync_ShouldReturnModels()
        {
            var response = await azureDigitalTwinServiceFixture.InMemoryAzureDigitalTwinReader.GetModelsAsync();

            Assert.Equal(azureDigitalTwinServiceFixture.Models.Count, response.Count());
        }

        [Fact]
        public async void GetRelationshipAsync_ShouldReturnRelationship()
        {
            var relationship = azureDigitalTwinServiceFixture.Relationships[GetRandomIndex(azureDigitalTwinServiceFixture.Relationships.Count)];

            var response = await azureDigitalTwinServiceFixture.InMemoryAzureDigitalTwinReader.GetRelationshipAsync(relationship.Id, relationship.SourceId);

            Assert.Equal(relationship.Id, response.Id);
        }

        [Fact]
        public async void GetRelationshipAsync_NonExisting_ShouldReturnNull()
        {
            var response = await azureDigitalTwinServiceFixture.InMemoryAzureDigitalTwinReader.GetRelationshipAsync(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());

            Assert.Null(response);
        }

        [Fact]
        public async void GetRelationshipsAsync_ShouldReturnRelationships()
        {
            var response = await azureDigitalTwinServiceFixture.InMemoryAzureDigitalTwinReader.GetTwinRelationshipsAsync(azureDigitalTwinServiceFixture.TwinWithRelationship.Id);

            Assert.True(response.Any());
        }

        [Fact]
        public async void GetRelationshipsAsync_NonExisting_ShouldReturnEmpty()
        {
            var response = await azureDigitalTwinServiceFixture.InMemoryAzureDigitalTwinReader.GetTwinRelationshipsAsync(Guid.NewGuid().ToString());

            Assert.Empty(response);
        }

        [Fact]
        public async void GetRelationshipsCountAsync_ShouldReturnCount()
        {
            var response = await azureDigitalTwinServiceFixture.InMemoryAzureDigitalTwinReader.GetRelationshipsCountAsync();

            Assert.Equal(azureDigitalTwinServiceFixture.Relationships.Count, response);
        }

        [Fact]
        public async void GetTwinsCountAsync_ShouldReturnCount()
        {
            var response = await azureDigitalTwinServiceFixture.InMemoryAzureDigitalTwinReader.GetTwinsCountAsync();

            Assert.Equal(azureDigitalTwinServiceFixture.Twins.Count, response);
        }

        [Fact(Skip = "To fix the BasicDigitalTwin deserialization for queries like Select twins from DigitalTwins twins")]
        public async void GetTwinsAsync_ShouldReturnTwins()
        {
            var sampleSize = 5;
            var twinIds = azureDigitalTwinServiceFixture.Twins.Take(sampleSize).Select(x => x.Id).ToList();
            twinIds.Add(Guid.NewGuid().ToString());

            var request = new GetTwinsInfoRequest();
            var response = await azureDigitalTwinServiceFixture.InMemoryAzureDigitalTwinReader.GetTwinsAsync(request, pageSize: sampleSize);

            Assert.Equal(sampleSize, response.Content.Count());
        }

        [Fact]
        public async virtual void QueryTwinsAsync_SelectAll_ShouldReturnTwins()
        {
            var query = "select * from digitaltwins";
            var response = await azureDigitalTwinServiceFixture.InMemoryAzureDigitalTwinReader.QueryTwinsAsync(query, pageSize: 100, continuationToken: null);

            Assert.Equal(azureDigitalTwinServiceFixture.Twins.Count, response.Content.Count());
            Assert.Null(response.ContinuationToken);
        }

        [Fact]
        public async void QueryTwinsAsync_WithRandomQuery_ThrowException()
        {
            await Assert.ThrowsAsync<Exception>(() => azureDigitalTwinServiceFixture.InMemoryAzureDigitalTwinReader.QueryTwinsAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()));
        }
    }
}
