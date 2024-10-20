namespace Connector.Nunit.Tests.IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web;
    using Connector.Nunit.Tests.Infrastructure.Extensions;
    using Connector.Nunit.Tests.TestData;
    using ConnectorCore.Common.Models;
    using ConnectorCore.Entities;
    using ConnectorCore.Models;
    using FluentAssertions;
    using Microsoft.Extensions.Options;
    using NUnit.Framework;

    public class EquipmentsTests
    {
        [Test]
        public async Task GetEquipmentsBySite_ReturnsEquipments()
        {
            using (var client = IntegrationFixture.Server.CreateClientRandomUser())
            {
                var result = await client.GetJsonAsync<GetEquipmentResult>("sites/6a8cb6ef-f23b-4608-a08b-0b779fd616cb/equipments");
                result.Data.Should().HaveCount(cnt => cnt >= 8);

                var equip1 = result.Data.First(q => q.Id == Constants.EquipmentId1);
                equip1.Tags.Should().NotBeEmpty();
                equip1.Tags.Should().Contain("Tag4");
            }
        }

        [Test]
        public async Task GetEquipmentsBySite_WithBadContToken_Returns_BadRequest()
        {
            using (var client = IntegrationFixture.Server.CreateClientRandomUser())
            {
                var ctoken = "1312231";
                var error = null as string;

                try
                {
                    var result = await client.GetJsonAsync<GetEquipmentResult>($"sites/6a8cb6ef-f23b-4608-a08b-0b779fd616cb/equipments?continuationToken={ctoken}");
                }
                catch (HttpRequestException ex)
                {
                    error = ex.ToString();
                }

                error.Should().NotBeNull();
                error.Should().Contain("400 (Bad Request)");
            }
        }

        [Test]
        public async Task GetEquipmentsBySite_WithWrongContToken_Returns_BadRequest()
        {
            using (var client = IntegrationFixture.Server.CreateClientRandomUser())
            {
                var ctoken = Guid.NewGuid();
                var error = null as string;

                try
                {
                    var result = await client.GetJsonAsync<GetEquipmentResult>($"sites/6a8cb6ef-f23b-4608-a08b-0b779fd616cb/equipments?continuationToken={ctoken}");
                }
                catch (HttpRequestException ex)
                {
                    error = ex.ToString();
                }

                error.Should().NotBeNull();
                error.Should().Contain("400 (Bad Request)");
            }
        }

        [Test]
        public async Task GetEquipmentsBySite_WithContToken_Returns_NextPage()
        {
            using (var client = IntegrationFixture.Server.CreateClientRandomUser())
            {
                var options = (IOptions<AppSettings>)IntegrationFixture.Server.Services.GetService(typeof(IOptions<AppSettings>));
                options.Value.EquipmentsPageSize = 1000;

                var equipmentId = Guid.NewGuid();

                var itemsAll = await client.GetJsonAsync<GetEquipmentResult>("sites/6a8cb6ef-f23b-4608-a08b-0b779fd616cb/equipments");
                itemsAll.Data.Should().NotBeEmpty();

                var equip1 = itemsAll.Data.First(q => q.Id == Constants.EquipmentId1);
                equip1.Tags.Should().NotBeEmpty();
                equip1.Tags.Should().Contain("Tag4");

                var pageSize = options.Value.EquipmentsPageSize = 3;

                var itemsPage1 = await client.GetJsonAsync<GetEquipmentResult>("sites/6a8cb6ef-f23b-4608-a08b-0b779fd616cb/equipments");
                itemsPage1.Data.Should().NotBeEmpty();
                itemsPage1.Data.Should().BeEquivalentTo(itemsAll.Data.Take(pageSize), q => q.WithStrictOrdering());
                itemsPage1.ContinuationToken.Should().NotBeEmpty();

                var itemsPage2 = await client.GetJsonAsync<GetEquipmentResult>($"sites/6a8cb6ef-f23b-4608-a08b-0b779fd616cb/equipments?continuationToken={itemsPage1.ContinuationToken}");
                itemsPage2.Data.Should().NotBeEmpty();
                itemsPage2.Data.Should().BeEquivalentTo(itemsAll.Data.Skip(pageSize).Take(pageSize), q => q.WithStrictOrdering());
            }
        }

        [Test]
        public async Task GetEquipmentsBySite_WithContToken_Returns_LastPageWithoutToken()
        {
            using (var client = IntegrationFixture.Server.CreateClientRandomUser())
            {
                var options = (IOptions<AppSettings>)IntegrationFixture.Server.Services.GetService(typeof(IOptions<AppSettings>));
                options.Value.EquipmentsPageSize = 1000;

                var equipmentId = Guid.NewGuid();

                var itemsAll = await client.GetJsonAsync<GetEquipmentResult>("sites/6a8cb6ef-f23b-4608-a08b-0b779fd616cb/equipments");
                itemsAll.Data.Should().NotBeEmpty();

                var pageSize = options.Value.EquipmentsPageSize = (itemsAll.Data.Count / 2) + 2;

                var itemsPage1 = await client.GetJsonAsync<GetEquipmentResult>("sites/6a8cb6ef-f23b-4608-a08b-0b779fd616cb/equipments");
                itemsPage1.Data.Should().NotBeEmpty();
                itemsPage1.Data.Should().BeEquivalentTo(itemsAll.Data.Take(pageSize), q => q.WithStrictOrdering());
                itemsPage1.ContinuationToken.Should().NotBeEmpty();

                var itemsPage2 = await client.GetJsonAsync<GetEquipmentResult>($"sites/6a8cb6ef-f23b-4608-a08b-0b779fd616cb/equipments?continuationToken={itemsPage1.ContinuationToken}");
                itemsPage2.Data.Should().NotBeEmpty();
                itemsPage2.Data.Should().BeEquivalentTo(itemsAll.Data.Skip(pageSize).Take(pageSize), q => q.WithStrictOrdering());
                itemsPage2.ContinuationToken.Should().BeNull();
            }
        }

        [Test]
        public async Task GetEquipment_ReturnsEquipment()
        {
            using (var client = IntegrationFixture.Server.CreateClientRandomUser())
            {
                var equipment = await client.GetJsonAsync<EquipmentEntity>($"equipments/{Constants.EquipmentId1:D}");
                equipment.Should().NotBeNull();
                equipment.Id.Should().Be(Constants.EquipmentId1);
                equipment.Tags.Should().NotBeNull();
                equipment.Tags.Should().NotBeEmpty();
                equipment.Tags.Should().Contain(t => t.Id == Constants.TagId4);
            }
        }

        [Test]
        public async Task GetEquipmentsList_ByIds_WithPointsAndPointTags_ReturnsEquipmentsWithPointsAndPointTags()
        {
            using (var client = IntegrationFixture.Server.CreateClientRandomUser())
            {
                var equipmentIds = string.Join(",", new[] { Constants.EquipmentId1, Constants.EquipmentId2 });
                var equipments = await client.GetJsonAsync<List<EquipmentEntity>>($"equipments?equipmentIds={equipmentIds}&includePoints=true&includePointTags=true");
                equipments.Should().NotBeEmpty();
                equipments.Should().Contain(eq => eq.Id == Constants.EquipmentId1);
                equipments.Should().Contain(eq => eq.Id == Constants.EquipmentId2);

                var equipment1 = equipments.First(eq => eq.Id == Constants.EquipmentId1);
                var equipment2 = equipments.First(eq => eq.Id == Constants.EquipmentId2);

                equipment1.Tags.Should().NotBeEmpty();
                equipment1.Tags.Should().HaveCount(1);
                equipment1.Tags.Should().Contain(t => t.Id == Constants.TagId4);

                equipment1.PointTags.Should().NotBeEmpty();
                equipment1.PointTags.Should().HaveCount(2);
                equipment1.PointTags.Should().Contain(t => t.Id == Constants.TagId4);
                equipment1.PointTags.Should().Contain(t => t.Id == Constants.TagId1);

                equipment1.Points.Should().HaveCount(2);
                equipment1.Points.Should().Contain(p => p.EntityId == Constants.PointId1);
                equipment1.Points.Should().Contain(p => p.EntityId == Constants.PointId2);

                equipment2.Points.Should().BeEmpty();
                equipment2.PointTags.Should().BeEmpty();

                equipment2.Tags.Should().NotBeEmpty();
                equipment2.Tags.Should().HaveCount(3);

                equipment2.Tags.Should().Contain(t => t.Id == Constants.TagForCategoryId1);
                equipment2.Tags.Should().Contain(t => t.Id == Constants.TagForCategoryId2);
                equipment2.Tags.Should().Contain(t => t.Id == Constants.TagForCategoryId3);
            }
        }

        [Test]
        public async Task GetEquipment_WithPointTags_ReturnsEquipmentWithPointTags()
        {
            using (var client = IntegrationFixture.Server.CreateClientRandomUser())
            {
                var equipment = await client.GetJsonAsync<EquipmentEntity>($"equipments/{Constants.EquipmentId1:D}?includePointTags=true");
                equipment.Should().NotBeNull();
                equipment.Id.Should().Be(Constants.EquipmentId1);
                equipment.Tags.Should().NotBeNull();
                equipment.Tags.Should().NotBeEmpty();
                equipment.Tags.Should().Contain(t => t.Id == Constants.TagId4);
                equipment.PointTags.Should().NotBeNull();
                equipment.PointTags.Should().NotBeEmpty();
                equipment.PointTags.Should().Contain(t => t.Id == Constants.TagId4);
                equipment.PointTags.Should().Contain(t => t.Id == Constants.TagId1);
            }
        }

        [Test]
        public async Task GetEquipmentByCategories_ReturnsEquipmentByCategories()
        {
            using (var client = IntegrationFixture.Server.CreateClientRandomUser())
            {
                var equipmentEntities = await client.GetJsonAsync<List<EquipmentEntity>>($"sites/{Constants.SiteIdDefault}/categories/{Constants.TagCategoryId1}/equipments");
                equipmentEntities.Should().NotBeNull();
                equipmentEntities.Should().Contain(t => t.Id == Constants.EquipmentId2);
            }
        }

        [Test]
        public async Task GetEquipmentsBySite_WrongFormatGuid_Returns400()
        {
            using (var client = IntegrationFixture.Server.CreateClientRandomUser())
            {
                var response = await client.GetAsync("sites/6a8cb6ef-f23b-4608-a08b-0b779fd616cq/equipments");
                response.IsSuccessStatusCode.Should().BeFalse();
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Test]
        public async Task GetEquipmentWithPoints_ReturnsEquipmentWithPoints()
        {
            using (var client = IntegrationFixture.Server.CreateClientRandomUser())
            {
                var equipment = await client.GetJsonAsync<EquipmentEntity>($"equipments/{Constants.EquipmentId1:D}?includePoints=true");
                equipment.Should().NotBeNull();
                equipment.Id.Should().Be(Constants.EquipmentId1);
                equipment.Points.Should().NotBeNull();
                equipment.Points.Should().NotBeEmpty();
                equipment.Points.Should().Contain(p => p.EntityId == Constants.PointId1);
                equipment.Points.Should().Contain(p => p.EntityId == Constants.PointId2);
                equipment.Points.Should().HaveCount(2);
            }
        }

        [Test]
        public async Task GetEquipmentsByFloor_ReturnsEquipments()
        {
            using (var client = IntegrationFixture.Server.CreateClientRandomUser())
            {
                var equipments = await client.GetJsonAsync<List<EquipmentEntity>>($"sites/{Constants.SiteIdDefault}/floors/{Constants.FloorId1}/equipments");
                equipments.Should().NotBeNull();
                equipments.Should().NotBeEmpty();
                equipments.Should().Contain(e => e.Id == Constants.EquipmentId6);
                equipments.Should().Contain(e => e.Id == Constants.EquipmentId7);
            }
        }

        [Test]
        public async Task GetEquipmentsByFloor_WithKeyword_ReturnsEquipments()
        {
            using (var client = IntegrationFixture.Server.CreateClientRandomUser())
            {
                var equipments = await client.GetJsonAsync<List<EquipmentEntity>>($"sites/{Constants.SiteIdDefault}/floors/{Constants.FloorId1}/equipments?keyword={HttpUtility.UrlEncode("child 2-2")}");
                equipments.Should().NotBeNull();
                equipments.Should().NotBeEmpty();
                equipments.Should().NotContain(e => e.Id == Constants.EquipmentId6);
                equipments.Should().Contain(e => e.Id == Constants.EquipmentId7);
            }
        }

        [Test]
        public async Task RefreshEquipmentCache_ReturnsOk()
        {
            using (var client = IntegrationFixture.Server.CreateClientRandomUser())
            {
                var response = await client.PostAsync("equipments/cache/refresh", null);
                response.StatusCode.Should().Be(HttpStatusCode.OK);
            }
        }
    }
}
