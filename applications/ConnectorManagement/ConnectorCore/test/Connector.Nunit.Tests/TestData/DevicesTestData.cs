namespace Connector.Nunit.Tests.TestData
{
    using System;
    using System.Collections.Generic;
    using ConnectorCore.Entities;

    public class DevicesTestData
    {
        public static List<DeviceEntity> Devices = new List<DeviceEntity>
        {
            new DeviceEntity
            {
                Id = Constants.DeviceId1,
                Name = "Device 1",
                SiteId = Constants.SiteIdDefault,
                ClientId = Guid.Parse("11bdf984-4fac-4b8a-abe9-a0fd6e8e2df3"),
                ConnectorId = Constants.ConnectorId1,
                RegistrationId = "ae6cc21c-a650-46d4-80f4-c8946474b628",
                RegistrationKey = "554bc48b-05a7-41be-bed9-dc9dd414089a",
                ExternalDeviceId = Constants.DeviceExternalId1,
                Metadata = "metadata",
                IsEnabled = true,
            },

            new DeviceEntity
            {
                Id = Constants.DeviceId2,
                Name = "Device 2",
                SiteId = Constants.SiteIdDefault,
                ClientId = Guid.Parse("11bdf984-4fac-4b8a-abe9-a0fd6e8e2df3"),
                ConnectorId = Constants.ConnectorId2,
                RegistrationId = "ae6cc21c-a650-46d4-80f4-c8946474b628",
                RegistrationKey = "554bc48b-05a7-41be-bed9-dc9dd414089a",
                ExternalDeviceId = Constants.DeviceExternalId2,
                Metadata = "metadata",
                IsEnabled = true,
            },

            new DeviceEntity
            {
                Id = Constants.DeviceId3,
                Name = "Device to be deleted",
                SiteId = Constants.SiteIdDefault,
                ClientId = Guid.Parse("11bdf984-4fac-4b8a-abe9-a0fd6e8e2df3"),
                ConnectorId = Constants.ConnectorId2,
                RegistrationId = "ae6cc21c-a650-46d4-80f4-c8946474b628",
                RegistrationKey = "554bc48b-05a7-41be-bed9-dc9dd414089a",
                ExternalDeviceId = Constants.DeviceExternalId3,
                Metadata = "metadata",
                IsEnabled = true,
            },
            new DeviceEntity
            {
                Id = Constants.DeviceIdForValidation,
                Name = "Device for validation",
                SiteId = Constants.SiteIdDefault,
                ClientId = Guid.Parse("11bdf984-4fac-4b8a-abe9-a0fd6e8e2df3"),
                ConnectorId = Constants.ConnectorIdForValidation,
                RegistrationId = "ae6cc21c-a650-46d4-80f4-c8946474b628",
                RegistrationKey = "554bc48b-05a7-41be-bed9-dc9dd414089a",
                ExternalDeviceId = Constants.DeviceExternalIdForValidation,
                Metadata = "metadata",
                IsEnabled = true,
            },
            new DeviceEntity
            {
                Id = Constants.DeviceIdForValidationNotFirst,
                Name = "Device for second validation",
                SiteId = Constants.SiteIdDefault,
                ClientId = Guid.Parse("11bdf984-4fac-4b8a-abe9-a0fd6e8e2df3"),
                ConnectorId = Constants.ConnectorIdForValidationNotFirst,
                RegistrationId = "ae6cc21c-a650-46d4-80f4-c8946474b628",
                RegistrationKey = "554bc48b-05a7-41be-bed9-dc9dd414089a",
                ExternalDeviceId = Constants.DeviceExternalIdForValidationNotFirst,
                Metadata = "metadata",
                IsEnabled = true,
            },
        };
    }
}
