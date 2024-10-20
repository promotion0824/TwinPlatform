namespace Connector.Nunit.Tests.IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Connector.Nunit.Tests.Infrastructure.Extensions;
    using Connector.Nunit.Tests.TestData;
    using ConnectorCore.Entities;
    using FluentAssertions;
    using NUnit.Framework;

    public class DevicesTests
    {
        [Test]
        public async Task GetDevicesByConnector_ReturnsDevices()
        {
            using (var client = IntegrationFixture.Server.CreateClientRandomUser())
            {
                var devices = await client.GetJsonAsync<List<DeviceEntity>>($"connectors/{Constants.ConnectorId2:D}/devices");
                foreach (var device in devices)
                {
                    device.Points.Should().BeNull();
                }

                devices.Select(d => d.Id).Should().Contain(new List<Guid> { Constants.DeviceId2 });
            }
        }

        [Test]
        public async Task GetDevicesByConnector_ReturnsDevices_CredentialsFromConnector()
        {
            using (var client = IntegrationFixture.Server.CreateClientRandomUser())
            {
                var devices = await client.GetJsonAsync<List<DeviceEntity>>($"connectors/{Constants.ConnectorId1:D}/devices");
                foreach (var device in devices)
                {
                    device.Points.Should().BeNull();
                }

                devices.Select(d => d.Id).Should().Contain(new List<Guid> { Constants.DeviceId1 });

                var connector1 = ConnectorsTestData.Connectors.First(q => q.Id == Constants.ConnectorId1);
                var device1 = devices.First(q => q.Id == Constants.DeviceId1);

                device1.RegistrationId.Should().Be(connector1.RegistrationId);
                device1.RegistrationKey.Should().Be(connector1.RegistrationKey);
            }
        }

        [Test]
        public async Task GetDevicesByConnector_IsEnabled_ReturnsEnabledDevices()
        {
            using (var client = IntegrationFixture.Server.CreateClientRandomUser())
            {
                var devices = await client.GetJsonAsync<List<DeviceEntity>>($"connectors/{Constants.ConnectorId2:D}/devices?isEnabled=true");
                devices.Select(d => d.Id).Should().Contain(new List<Guid> { Constants.DeviceId2 });
            }
        }

        [Test]
        public async Task GetDevicesByConnector_NotIsEnabled_DoesNotReturnEnabledDevices()
        {
            using (var client = IntegrationFixture.Server.CreateClientRandomUser())
            {
                var devices = await client.GetJsonAsync<List<DeviceEntity>>($"connectors/{Constants.ConnectorId2:D}/devices?isEnabled=false");
                devices.Select(d => d.Id).Should().NotContain(new List<Guid> { Constants.DeviceId2 });
            }
        }

        [Test]
        public async Task GetDevicesByConnectorIncludePoints_ReturnsDevices()
        {
            using (var client = IntegrationFixture.Server.CreateClientRandomUser())
            {
                var devices = await client.GetJsonAsync<List<DeviceEntity>>($"connectors/{Constants.ConnectorId1:D}/devices?includepoints=true");
                devices.Count(d => d.Points != null).Should().BeGreaterOrEqualTo(1);
                devices.Select(d => d.Id).Should().Contain(new List<Guid> { Constants.DeviceId1 });
            }
        }

        [Test]
        public async Task GetDeviceByConnector_ReturnsDevice()
        {
            using (var client = IntegrationFixture.Server.CreateClientRandomUser())
            {
                var device = await client.GetJsonAsync<DeviceEntity>($"connectors/{Constants.ConnectorId2:D}/devices/{Constants.DeviceId2:D}");
                device.Id.Should().Be(Constants.DeviceId2);
            }
        }

        [Test]
        public async Task GetDeviceByConnector_ReturnsDevice_CredentialsFromConnector()
        {
            using (var client = IntegrationFixture.Server.CreateClientRandomUser())
            {
                var device = await client.GetJsonAsync<DeviceEntity>($"connectors/{Constants.ConnectorId1:D}/devices/{Constants.DeviceId1:D}");
                device.Id.Should().Be(Constants.DeviceId1);

                var connector1 = ConnectorsTestData.Connectors.First(q => q.Id == Constants.ConnectorId1);

                device.RegistrationId.Should().Be(connector1.RegistrationId);
                device.RegistrationKey.Should().Be(connector1.RegistrationKey);
            }
        }

        [Test]
        public async Task GetDeviceByExternalPointId_ReturnsDevice()
        {
            using (var client = IntegrationFixture.Server.CreateClientRandomUser())
            {
                var device = await client.GetJsonAsync<DeviceEntity>($"sites/6a8cb6ef-f23b-4608-a08b-0b779fd616cb/devices/externalPointId/{Constants.PointExternalId1}");
                device.Id.Should().Be(Constants.DeviceId1);
            }
        }

        [Test]
        public async Task GetDeviceByExternalPointId_ReturnsDevice_CredentialsFromConnector()
        {
            using (var client = IntegrationFixture.Server.CreateClientRandomUser())
            {
                var connector1 = ConnectorsTestData.Connectors.First(q => q.Id == Constants.ConnectorId1);
                var device1 = DevicesTestData.Devices.First(q => q.Id == Constants.DeviceId1);

                var device = await client.GetJsonAsync<DeviceEntity>($"sites/6a8cb6ef-f23b-4608-a08b-0b779fd616cb/devices/externalPointId/{Constants.PointExternalId1}");
                device.Id.Should().Be(Constants.DeviceId1);
                device.RegistrationId.Should().NotBe(device1.RegistrationId);
                device.RegistrationKey.Should().NotBe(device1.RegistrationKey);
                device.RegistrationId.Should().Be(connector1.RegistrationId);
                device.RegistrationKey.Should().Be(connector1.RegistrationKey);
            }
        }

        [Test]
        public async Task GetDeviceByExternalPointId_NotExists_ReturnsNotFound()
        {
            using (var client = IntegrationFixture.Server.CreateClientRandomUser())
            {
                var response = await client.GetAsync("sites/6a8cb6ef-f23b-4608-a08b-0b779fd616cb/devices/externalPointId/37e7a71c-fa4c-4da1-a34a-84ad56160368");
                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Test]
        public async Task GetDeviceByConnector_BadConnectorId_ReturnsNotFound()
        {
            using (var client = IntegrationFixture.Server.CreateClientRandomUser())
            {
                var deviceRespose = await client.GetAsync($"connectors/{Guid.NewGuid():D}/devices/{Constants.DeviceId2:D}");
                deviceRespose.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Test]
        public async Task GetDeviceByConnector_WrongFormatGuid_Returns400()
        {
            using (var client = IntegrationFixture.Server.CreateClientRandomUser())
            {
                var response = await client.GetAsync($"connectors/{Constants.ConnectorId2:D}/devices/3eb68bff-d530-4da8-b537-db3d48fde21");
                response.IsSuccessStatusCode.Should().BeFalse();
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Test]
        public async Task GetDeviceByConnector_WrongGuid_Returns404()
        {
            using (var client = IntegrationFixture.Server.CreateClientRandomUser())
            {
                var response = await client.GetAsync($"connectors/{Constants.ConnectorId2:D}/devices/{Guid.NewGuid():D}");
                response.IsSuccessStatusCode.Should().BeFalse();
                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Test]
        public async Task GetDevices_ReturnsDevices()
        {
            using (var client = IntegrationFixture.Server.CreateClientRandomUser())
            {
                var devices = await client.GetJsonAsync<List<DeviceEntity>>("sites/6a8cb6ef-f23b-4608-a08b-0b779fd616cb/devices");
                devices.Select(d => d.Id).Should().Contain(new List<Guid> { Constants.DeviceId1, Constants.DeviceId2 });
            }
        }

        [Test]
        public async Task GetDevices_ReturnsDevices_CredentialsFromConnector()
        {
            using (var client = IntegrationFixture.Server.CreateClientRandomUser())
            {
                var devices = await client.GetJsonAsync<List<DeviceEntity>>("sites/6a8cb6ef-f23b-4608-a08b-0b779fd616cb/devices");
                devices.Select(d => d.Id).Should().Contain(new List<Guid> { Constants.DeviceId1, Constants.DeviceId2 });

                var connector1 = ConnectorsTestData.Connectors.First(q => q.Id == Constants.ConnectorId1);
                var device = devices.First(q => q.Id == Constants.DeviceId1);
                device.RegistrationId.Should().Be(connector1.RegistrationId);
                device.RegistrationKey.Should().Be(connector1.RegistrationKey);
            }
        }

        [Test]
        public async Task GetDevices_IsEnabled_ReturnsEnabledDevices()
        {
            using (var client = IntegrationFixture.Server.CreateClientRandomUser())
            {
                var devices = await client.GetJsonAsync<List<DeviceEntity>>("sites/6a8cb6ef-f23b-4608-a08b-0b779fd616cb/devices?isEnabled=true");
                devices.Select(d => d.Id).Should().Contain(new List<Guid> { Constants.DeviceId1, Constants.DeviceId2 });
            }
        }

        [Test]
        public async Task GetDevices_NotIsEnabled_DoesNotReturnEbabledDevice()
        {
            using (var client = IntegrationFixture.Server.CreateClientRandomUser())
            {
                var devices = await client.GetJsonAsync<List<DeviceEntity>>("sites/6a8cb6ef-f23b-4608-a08b-0b779fd616cb/devices?isEnabled=false");
                devices.Select(d => d.Id).Should().NotContain(new List<Guid> { Constants.DeviceId1, Constants.DeviceId2 });
            }
        }

        [Test]
        public async Task GetDevice_ReturnsDevice()
        {
            using (var client = IntegrationFixture.Server.CreateClientRandomUser())
            {
                var device = await client.GetJsonAsync<DeviceEntity>("sites/6a8cb6ef-f23b-4608-a08b-0b779fd616cb/devices/ca32df47-d84c-4aef-b18d-1b2fa5cd6868");
                device.Id.Should().Be(Guid.Parse("ca32df47-d84c-4aef-b18d-1b2fa5cd6868"));
                device.IsEnabled.Should().BeTrue();
            }
        }

        [Test]
        public async Task GetDevice_ReturnsDevice_CredentialsFromConnector()
        {
            using (var client = IntegrationFixture.Server.CreateClientRandomUser())
            {
                var device = await client.GetJsonAsync<DeviceEntity>($"sites/6a8cb6ef-f23b-4608-a08b-0b779fd616cb/devices/{Constants.DeviceId1}");
                device.Id.Should().Be(Constants.DeviceId1);
                device.IsEnabled.Should().BeTrue();

                var connector1 = ConnectorsTestData.Connectors.First(q => q.Id == Constants.ConnectorId1);
                device.RegistrationId.Should().Be(connector1.RegistrationId);
                device.RegistrationKey.Should().Be(connector1.RegistrationKey);
            }
        }

        [Test]
        public async Task GetDevice_BadSiteId_ReturnsNotFound()
        {
            using (var client = IntegrationFixture.Server.CreateClientRandomUser())
            {
                var deviceRespose = await client.GetAsync($"sites/{Guid.NewGuid():D}/devices/ca32df47-d84c-4aef-b18d-1b2fa5cd6868");
                deviceRespose.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Test]
        public async Task GetDevice_WrongFormatGuid_Returns400()
        {
            using (var client = IntegrationFixture.Server.CreateClientRandomUser())
            {
                var response = await client.GetAsync("sites/6a8cb6ef-f23b-4608-a08b-0b779fd616cb/devices/ca32df47-d84c-4aef-b18d-1b2fa5cd686");
                response.IsSuccessStatusCode.Should().BeFalse();
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Test]
        public async Task GetDevice_WrongGuid_Returns404()
        {
            using (var client = IntegrationFixture.Server.CreateClientRandomUser())
            {
                var response = await client.GetAsync("sites/6a8cb6ef-f23b-4608-a08b-0b779fd616cb/devices/ca32df47-d84c-4aef-b18d-1b2fa5cd686a");
                response.IsSuccessStatusCode.Should().BeFalse();
                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Test]
        public async Task GetDeviceWithPoints_ReturnsDeviceWithPoints()
        {
            using (var client = IntegrationFixture.Server.CreateClientRandomUser())
            {
                var device = await client.GetJsonAsync<DeviceEntity>("sites/6a8cb6ef-f23b-4608-a08b-0b779fd616cb/devices/ca32df47-d84c-4aef-b18d-1b2fa5cd6868?includePoints=true");
                device.Id.Should().Be(Guid.Parse("ca32df47-d84c-4aef-b18d-1b2fa5cd6868"));
                device.Points.Should().NotBeNull();
                device.Points.Should().NotBeEmpty();
                device.Points.Should().HaveCount(3);
                device.Points.Should().Contain(p => p.EntityId == Constants.PointId1);
                device.Points.Should().Contain(p => p.EntityId == Constants.PointId2);
                device.Points.Should().Contain(p => p.EntityId == Constants.PointId4);
            }
        }

        [Test]
        public async Task UpdateDevice_WrongGuid_Returns404()
        {
            var updatedDevice = new DeviceEntity
            {
                Id = Guid.NewGuid(),
                Name = $"Updated name {Guid.NewGuid():N}",
                ClientId = Guid.Parse("11bdf984-4fac-4b8a-abe9-a0fd6e8e2df3"),
                ConnectorId = Guid.Parse("360d2af2-446b-4b1f-bb6f-20af553ea289"),
                RegistrationId = "ae6cc21c-a650-46d4-80f4-c8946474b628",
                RegistrationKey = "554bc48b-05a7-41be-bed9-dc9dd414089a",
                ExternalDeviceId = "0b6144e1-0f63-4554-8c7b-8fc1e1eb20e7",
                Metadata = @"{""Name"": ""NameValue""}",
            };

            using (var client = IntegrationFixture.Server.CreateClientRandomUser())
            {
                var httpContent = new FormUrlEncodedContent(updatedDevice.ToFormData(true).AsEnumerable());
                var responseUpdatedDevice = await client.PutAsync("sites/6a8cb6ef-f23b-4608-a08b-0b779fd616cb/devices", httpContent);
                responseUpdatedDevice.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }
    }
}
