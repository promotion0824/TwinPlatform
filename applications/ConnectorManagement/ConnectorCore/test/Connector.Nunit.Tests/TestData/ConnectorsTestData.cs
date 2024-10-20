namespace Connector.Nunit.Tests.TestData
{
    using System;
    using System.Collections.Generic;
    using ConnectorCore.Entities;

    public class ConnectorsTestData
    {
        public static List<ConnectorTypeEntity> Types = new List<ConnectorTypeEntity>
        {
            new ConnectorTypeEntity
            {
                Id = Constants.ConnectorTypeId1,
                Name = "Connector type 1",
                ConnectorConfigurationSchemaId = Guid.Parse("bbe1a34c-1322-4f5f-a30b-b1f0bfab2a9a"),
                DeviceMetadataSchemaId = Guid.Parse("e6c7ae20-2ff2-4d90-859d-89e01ebbc26c"),
                PointMetadataSchemaId = Guid.Parse("30a3db15-6eff-4e82-ac09-2e7b52040e54"),
            },
            new ConnectorTypeEntity
            {
                Id = Constants.ConnectorTypeId2,
                Name = "Connector type 2",
                ConnectorConfigurationSchemaId = Guid.Parse("bbe1a34c-1322-4f5f-a30b-b1f0bfab2a9a"),
                DeviceMetadataSchemaId = Guid.Parse("e6c7ae20-2ff2-4d90-859d-89e01ebbc26c"),
                PointMetadataSchemaId = Guid.Parse("30a3db15-6eff-4e82-ac09-2e7b52040e54"),
            },
            new ConnectorTypeEntity
            {
                Id = Constants.ConnectorTypeId3,
                Name = "Connector type tp be deleted",
                ConnectorConfigurationSchemaId = Guid.Parse("bbe1a34c-1322-4f5f-a30b-b1f0bfab2a9a"),
                DeviceMetadataSchemaId = Guid.Parse("e6c7ae20-2ff2-4d90-859d-89e01ebbc26c"),
                PointMetadataSchemaId = Guid.Parse("30a3db15-6eff-4e82-ac09-2e7b52040e54"),
            },
            new ConnectorTypeEntity
            {
                Id = Constants.ConnectorTypeId4,
                Name = "Connector type 4",
                ConnectorConfigurationSchemaId = Guid.Parse("bbe1a34c-1322-4f5f-a30b-b1f0bfab2a9a"),
                DeviceMetadataSchemaId = Guid.Parse("e6c7ae20-2ff2-4d90-859d-89e01ebbc26c"),
                PointMetadataSchemaId = Guid.Parse("30a3db15-6eff-4e82-ac09-2e7b52040e54"),
            },
            new ConnectorTypeEntity
            {
                Id = Constants.ConnectorTypeId5,
                Name = "Connector type 5",
                ConnectorConfigurationSchemaId = Guid.Parse("bbe1a34c-1322-4f5f-a30b-b1f0bfab2a9a"),
                DeviceMetadataSchemaId = Guid.Parse("e6c7ae20-2ff2-4d90-859d-89e01ebbc26c"),
                PointMetadataSchemaId = Guid.Parse("30a3db15-6eff-4e82-ac09-2e7b52040e54"),
            },
        };

        public static List<ConnectorEntity> Connectors = new List<ConnectorEntity>
        {
            new ConnectorEntity
            {
                Id = Constants.ConnectorId1,
                Name = "Connector 1",
                ClientId = Constants.ClientIdDefault,
                SiteId = Constants.SiteIdDefault,
                Configuration = string.Empty,
                ConnectorTypeId = Constants.ConnectorTypeId1,
                IsEnabled = true,
                IsLoggingEnabled = true,
                ErrorThreshold = 5,
                RegistrationId = "0e1e581e-b47f-432d-bfae-d334eb0a3ded",
                RegistrationKey = "91301a50-b347-4846-a6c8-6a060a7079df",
            },

            new ConnectorEntity
            {
                Id = Constants.ConnectorId2,
                Name = "Connector 2",
                ClientId = Constants.ClientIdDefault,
                SiteId = Constants.SiteIdDefault,
                Configuration = string.Empty,
                ConnectorTypeId = Constants.ConnectorTypeId1,
                IsEnabled = true,
                IsLoggingEnabled = true,
                ErrorThreshold = 0,
            },

            new ConnectorEntity
            {
                Id = Constants.ConnectorId3ToDelete,
                Name = "Connector to be deleted",
                ClientId = Constants.ClientIdDefault,
                SiteId = Constants.SiteIdDefault,
                Configuration = string.Empty,
                ConnectorTypeId = Constants.ConnectorTypeId1,
                IsEnabled = true,
                IsLoggingEnabled = true,
                ErrorThreshold = 0,
            },

            new ConnectorEntity
            {
                Id = Constants.ConnectorId4ToDelete,
                Name = "Connector to be deleted 2",
                ClientId = Constants.ClientIdDefault,
                SiteId = Constants.SiteIdDefault,
                Configuration = string.Empty,
                ConnectorTypeId = Constants.ConnectorTypeId1,
                IsEnabled = true,
                IsLoggingEnabled = true,
                ErrorThreshold = 0,
            },

            new ConnectorEntity
            {
                Id = Constants.ConnectorId5,
                Name = "Connector 5",
                ClientId = Constants.ClientIdDefault,
                SiteId = Constants.SiteIdDefault,
                Configuration = string.Empty,
                ConnectorTypeId = Constants.ConnectorTypeId4,
                IsEnabled = true,
                IsLoggingEnabled = true,
                ErrorThreshold = 5,
            },

            new ConnectorEntity
            {
                Id = Constants.ConnectorId6,
                Name = "Connector 6",
                ClientId = Constants.ClientIdDefault,
                SiteId = Constants.SiteIdDefault,
                Configuration = string.Empty,
                ConnectorTypeId = Constants.ConnectorTypeId5,
                IsEnabled = true,
                IsLoggingEnabled = true,
                ErrorThreshold = 5,
            },

            new ConnectorEntity
            {
                Id = Constants.ConnectorId7,
                Name = "Connector 7",
                ClientId = Constants.ClientIdDefault,
                SiteId = Constants.SiteIdDefault,
                Configuration = string.Empty,
                ConnectorTypeId = Constants.ConnectorTypeId5,
                IsEnabled = false,
                IsLoggingEnabled = true,
                ErrorThreshold = 5,
            },
            new ConnectorEntity
            {
                Id = Constants.ConnectorIdForValidation,
                Name = "Connector to provide validation data",
                ClientId = Constants.ClientIdDefault,
                SiteId = Constants.SiteIdDefault,
                Configuration = string.Empty,
                ConnectorTypeId = Constants.ConnectorTypeId5,
                IsEnabled = false,
                IsLoggingEnabled = true,
                ErrorThreshold = 5,
            },
            new ConnectorEntity
            {
                Id = Constants.ConnectorIdForValidationNotFirst,
                Name = "Connector to provide validation data(not first import)",
                ClientId = Constants.ClientIdDefault,
                SiteId = Constants.SiteIdDefault,
                Configuration = string.Empty,
                ConnectorTypeId = Constants.ConnectorTypeId5,
                IsEnabled = false,
                IsLoggingEnabled = true,
                ErrorThreshold = 5,
            },
        };
    }
}
