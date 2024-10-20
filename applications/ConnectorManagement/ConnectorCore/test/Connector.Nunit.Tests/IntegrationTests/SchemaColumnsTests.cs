namespace Connector.Nunit.Tests.IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Connector.Nunit.Tests.Infrastructure.Extensions;
    using Connector.Nunit.Tests.TestData;
    using ConnectorCore.Entities;
    using ConnectorCore.Services;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using NUnit.Framework;

    public class SchemaColumnsTests
    {
        private async Task<SchemaColumnEntity> CreateSchemaColumn_ReturnsNewColumnAsync()
        {
            using (var client = IntegrationFixture.Server.CreateClientRandomUser())
            {
                var newSchemaColumn = new SchemaColumnEntity
                {
                    Name = "Creating Schema",
                    DataType = "String",

                    //GroupName = "Group N",
                    IsRequired = true,

                    //OrderInGroup = 1
                };
                var response = await client.PostFormAsync<SchemaColumnEntity>($"schemas/{Constants.SchemaId2:D}/schemacolumns", newSchemaColumn);
                return response;
            }
        }

        [Test]
        public async Task GetSchemaColumns_ReturnsSchemaColumns()
        {
            using (var client = IntegrationFixture.Server.CreateClientRandomUser())
            {
                var schemas = await client.GetJsonAsync<List<SchemaColumnEntity>>($"schemas/{Constants.SchemaId1:D}/schemacolumns");
                schemas.Should().HaveCount(cnt => cnt >= 1 && cnt <= 3);
                schemas.Should().OnlyContain(s => s.SchemaId == Constants.SchemaId1);
            }
        }

        [Test]
        public async Task GetSchemaColumn_ReturnsSchemaColumn()
        {
            using (var client = IntegrationFixture.Server.CreateClientRandomUser())
            {
                var column = await client.GetJsonAsync<SchemaColumnEntity>($"schemas/{Constants.SchemaId1:D}/schemacolumns/91cd8283-b7e9-4b45-bd0d-b443915ae87f");
                column.Id.Should().Be(Guid.Parse("91cd8283-b7e9-4b45-bd0d-b443915ae87f"));
            }
        }

        [Test]
        public async Task GetSchemaColumn_WrongFormatGuid_Returns400()
        {
            using (var client = IntegrationFixture.Server.CreateClientRandomUser())
            {
                var response = await client.GetAsync($"schemas/{Constants.SchemaId1:D}/schemacolumns/91cd8283-b7e9-4b45-bd0d-b443915ae8f");
                response.IsSuccessStatusCode.Should().BeFalse();
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Test]
        public async Task GetSchemaColumn_WrongGuid_Returns404()
        {
            using (var client = IntegrationFixture.Server.CreateClientRandomUser())
            {
                var response = await client.GetAsync($"schemas/{Constants.SchemaId1:D}/schemacolumns/91cd8283-b7e9-4b45-bd0d-b443915ae87a");
                response.IsSuccessStatusCode.Should().BeFalse();
                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Test]
        public async Task GetSchemaColumn_WrongSchemaId_Returns400()
        {
            using (var client = IntegrationFixture.Server.CreateClientRandomUser())
            {
                var response = await client.GetAsync($"schemas/{Constants.SchemaId2:D}/schemacolumns/91cd8283-b7e9-4b45-bd0d-b443915ae87f");
                response.IsSuccessStatusCode.Should().BeFalse();
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Test]
        public async Task CreateSchemaColumn_ReturnsNewColumn()
        {
            using (var client = IntegrationFixture.Server.CreateClientRandomUser())
            {
                var newSchemaColumn = new SchemaColumnEntity
                {
                    Name = "Creating Schema",
                    DataType = "String",
                    IsRequired = true,
                    SchemaId = Constants.SchemaId2,
                };
                var response = await client.PostFormAsync<SchemaColumnEntity>($"schemas/{Constants.SchemaId2:D}/schemacolumns", newSchemaColumn);
                response.SchemaId.Should().Be(Constants.SchemaId2);
                response.Id.Should().NotBe(Guid.Empty);
            }
        }

        [Test]
        public async Task CreateSchemaColumn_WrongEntitySchemaId_Returns400()
        {
            using (var client = IntegrationFixture.Server.CreateClientRandomUser())
            {
                var newSchemaColumn = new SchemaColumnEntity
                {
                    Name = "Creating Schema",
                    DataType = "String",
                    IsRequired = true,
                    SchemaId = Constants.SchemaId1,
                };
                var response = await client.PostFormAsync($"schemas/{Constants.SchemaId2:D}/schemacolumns", newSchemaColumn);
                response.IsSuccessStatusCode.Should().BeFalse();
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Test]
        public async Task GetSchemaTemplate_ReturnsTemplate()
        {
            var dataGenerator = IntegrationFixture.Server.Services.GetRequiredService<IJsonSchemaDataGenerator>();
            var columns = TestData.SchemaColumnsTestData.SchemaColumns.Where(c => c.SchemaId == Constants.SchemaId1).ToList();
            var expectedJson = dataGenerator.GenerateEmptyObject(columns);
            var expectedJobject = JsonConvert.DeserializeObject<JObject>(expectedJson);

            using (var client = IntegrationFixture.Server.CreateClientRandomUser())
            {
                var response = await client.GetAsync($"schemas/{Constants.SchemaId1}/template");
                response.IsSuccessStatusCode.Should().BeTrue();

                var strData = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<JObject>(strData);
                data.Should().BeEquivalentTo(expectedJobject);
            }
        }
    }
}
