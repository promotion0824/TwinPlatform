namespace Connector.Nunit.Tests.TestData
{
    using System.Collections.Generic;
    using ConnectorCore.Entities;

    public class PointsTestData
    {
        public static List<PointEntity> Points = new List<PointEntity>
        {
            new PointEntity
            {
                EntityId = Constants.PointId1,
                Name = "Point 1",
                Type = 3,
                Unit = "Unit1",
                SiteId = Constants.SiteIdDefault,
                ClientId = Constants.ClientIdDefault,
                DeviceId = Constants.DeviceId1,
                ExternalPointId = Constants.PointExternalId1,
                Metadata = "05b09f87-5941-4cf2-8b8f-a56e1225c76a",
                IsEnabled = true,
            },

            new PointEntity
            {
                EntityId = Constants.PointId2,
                Name = "Point 2",
                Type = 3,
                Unit = "Unit1",
                SiteId = Constants.SiteIdDefault,
                ClientId = Constants.ClientIdDefault,
                DeviceId = Constants.DeviceId1,
                ExternalPointId = Constants.PointExternalId2,
                Metadata = "05b09f87-5941-4cf2-8b8f-a56e1225c76a",
                IsEnabled = true,
            },

            new PointEntity
            {
                EntityId = Constants.PointId3,
                Name = "Point 3 to be deleted",
                Type = 3,
                Unit = "Unit1",
                SiteId = Constants.SiteIdDefault,
                ClientId = Constants.ClientIdDefault,
                DeviceId = Constants.DeviceId2,
                ExternalPointId = Constants.PointExternalId3,
                Metadata = "05b09f87-5941-4cf2-8b8f-a56e1225c76a",
                IsEnabled = true,
            },

            new PointEntity
            {
                EntityId = Constants.PointId4,
                Name = "Point 4",
                Type = 3,
                Unit = "Unit4",
                SiteId = Constants.SiteIdDefault,
                ClientId = Constants.ClientIdDefault,
                DeviceId = Constants.DeviceId1,
                ExternalPointId = Constants.PointExternalId4,
                Metadata = "05b09f87-5941-4cf2-8b8f-a56e1225c76a",
                IsEnabled = true,
            },
            new PointEntity
            {
                EntityId = Constants.PointId5ForValidation,
                Name = "Point 5 for validation",
                Type = 1,
                Unit = "Unit5",
                SiteId = Constants.SiteIdDefault,
                ClientId = Constants.ClientIdDefault,
                DeviceId = Constants.DeviceIdForValidationNotFirst,
                ExternalPointId = Constants.PointExternalId5,
                Metadata = "05b09f87-5941-4cf2-8b8f-a56e1225c76a",
                IsEnabled = true,
            },
            new PointEntity
            {
                EntityId = Constants.PointId6ForValidation,
                Name = "Point 6 for validation",
                Type = 2,
                Unit = "Unit6",
                SiteId = Constants.SiteIdDefault,
                ClientId = Constants.ClientIdDefault,
                DeviceId = Constants.DeviceIdForValidationNotFirst,
                ExternalPointId = Constants.PointExternalId6,
                Metadata = "05b09f87-5941-4cf2-8b8f-a56e1225c76a",
                IsEnabled = true,
            },
            new PointEntity
            {
                EntityId = Constants.PointId7ForValidation,
                Name = "Point 7 for validation",
                Type = 3,
                Unit = "Unit7",
                SiteId = Constants.SiteIdDefault,
                ClientId = Constants.ClientIdDefault,
                DeviceId = Constants.DeviceIdForValidationNotFirst,
                ExternalPointId = Constants.PointExternalId7,
                Metadata = "05b09f87-5941-4cf2-8b8f-a56e1225c76a",
                IsEnabled = true,
            },
        };
    }
}
