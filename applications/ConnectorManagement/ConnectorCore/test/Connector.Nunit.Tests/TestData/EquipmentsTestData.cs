namespace Connector.Nunit.Tests.TestData
{
    using System;
    using System.Collections.Generic;
    using ConnectorCore.Entities;
    using ConnectorCore.Models;

    public class EquipmentsTestData
    {
        public static List<EquipmentEntity> Equipments = new List<EquipmentEntity>
        {
            new EquipmentEntity
            {
                Id = Constants.EquipmentId1,
                Name = "Equipment synthetic root",
                SiteId = Constants.SiteIdDefault,
                Category = Constants.EquipmentCategory1,

                //LocationId = Constants.LocationIdSynthetic,
                ParentId = null,
                ClientId = Guid.Parse("efbef593-e84d-4fb6-a381-f6da4a66e716"),
                ExternalEquipmentId = "a17b41f4-a0ee-44ac-aa4b-2c036f3ac364",
            },

            new EquipmentEntity
            {
                Id = Constants.EquipmentId2,
                Name = "Equipment parent 1",
                SiteId = Constants.SiteIdDefault,
                Category = Constants.EquipmentCategory1,

                //LocationId = Constants.LocationIdPrimary,
                ParentId = Constants.EquipmentId1,
                ClientId = Guid.Parse("efbef593-e84d-4fb6-a381-f6da4a66e716"),
                ExternalEquipmentId = "a17b41f4-a0ee-44ac-aa4b-2c036f3ac364",
            },

            new EquipmentEntity
            {
                Id = Constants.EquipmentId3,
                Name = "Equipment parent 2",
                SiteId = Constants.SiteIdDefault,
                Category = Constants.EquipmentCategory1,

                //LocationId = Constants.LocationIdSecondary,
                ParentId = Constants.EquipmentId1,
                ClientId = Guid.Parse("efbef593-e84d-4fb6-a381-f6da4a66e716"),
                ExternalEquipmentId = "a17b41f4-a0ee-44ac-aa4b-2c036f3ac364",
            },

            new EquipmentEntity
            {
                Id = Constants.EquipmentId4,
                Name = "Equipment child 1-1",
                SiteId = Constants.SiteIdDefault,
                Category = Constants.EquipmentCategory2,

                //LocationId = Constants.LocationIdPrimary,
                ParentId = Guid.Parse("e3241d49-f9fb-409a-8be9-d535414fb1f5"),
                ClientId = Guid.Parse("efbef593-e84d-4fb6-a381-f6da4a66e716"),
                ExternalEquipmentId = "a17b41f4-a0ee-44ac-aa4b-2c036f3ac364",
            },

            new EquipmentEntity
            {
                Id = Constants.EquipmentId5,
                Name = "Equipment child 1-2",
                SiteId = Constants.SiteIdDefault,
                Category = Constants.EquipmentCategory3,

                //LocationId = Constants.LocationIdPrimary,
                ParentId = Guid.Parse("e3241d49-f9fb-409a-8be9-d535414fb1f5"),
                ClientId = Guid.Parse("efbef593-e84d-4fb6-a381-f6da4a66e716"),
                ExternalEquipmentId = "a17b41f4-a0ee-44ac-aa4b-2c036f3ac364",
            },

            new EquipmentEntity
            {
                Id = Constants.EquipmentId6,
                Name = "Equipment child 2-1",
                SiteId = Constants.SiteIdDefault,
                Category = Constants.EquipmentCategory3,

                //LocationId = Constants.LocationIdSecondary,
                ParentId = Guid.Parse("10f5ed7d-1bdf-494f-a327-02f7f50fb09d"),
                ClientId = Guid.Parse("efbef593-e84d-4fb6-a381-f6da4a66e716"),
                ExternalEquipmentId = "a17b41f4-a0ee-44ac-aa4b-2c036f3ac364",
                FloorId = Constants.FloorId1,
            },

            new EquipmentEntity
            {
                Id = Constants.EquipmentId7,
                Name = "Equipment child 2-2",
                SiteId = Constants.SiteIdDefault,
                Category = Constants.EquipmentCategory2,

                //LocationId = Constants.LocationIdSecondary,
                ParentId = Guid.Parse("10f5ed7d-1bdf-494f-a327-02f7f50fb09d"),
                ClientId = Guid.Parse("efbef593-e84d-4fb6-a381-f6da4a66e716"),
                ExternalEquipmentId = "a17b41f4-a0ee-44ac-aa4b-2c036f3ac364",
                FloorId = Constants.FloorId1,
            },

            new EquipmentEntity
            {
                Id = Constants.EquipmentId8,
                Name = "Equipment child 2-3 to be deleted",
                SiteId = Constants.SiteIdDefault,
                Category = Constants.EquipmentCategory3,

                //LocationId = Constants.LocationIdSecondary,
                ParentId = Guid.Parse("10f5ed7d-1bdf-494f-a327-02f7f50fb09d"),
                ClientId = Guid.Parse("efbef593-e84d-4fb6-a381-f6da4a66e716"),
                ExternalEquipmentId = "a17b41f4-a0ee-44ac-aa4b-2c036f3ac364",
            },

            new EquipmentEntity
            {
                Id = Constants.EquipmentId9,
                Name = "Equipment child 2-4 to be deleted",
                SiteId = Constants.SiteIdDefault,
                Category = Constants.EquipmentCategory2,

                //LocationId = Constants.LocationIdSecondary,
                ParentId = Guid.Parse("10f5ed7d-1bdf-494f-a327-02f7f50fb09d"),
                ClientId = Guid.Parse("efbef593-e84d-4fb6-a381-f6da4a66e716"),
                ExternalEquipmentId = "a17b41f4-a0ee-44ac-aa4b-2c036f3ac364",
            },

            new EquipmentEntity
            {
                Id = Constants.EquipmentId10ForValidation,
                Name = "Equipment child 2-4 to be deleted",
                SiteId = Constants.SiteIdDefault,
                Category = Constants.EquipmentCategory2,

                //LocationId = Constants.LocationIdSecondary,
                ParentId = Guid.Parse("10f5ed7d-1bdf-494f-a327-02f7f50fb09d"),
                ClientId = Guid.Parse("efbef593-e84d-4fb6-a381-f6da4a66e716"),
                ExternalEquipmentId = "a17b41f4-a0ee-44ac-aa4b-2c036f3ac364",
            },
        };

        public static List<EquipmentToPointLink> EquipmentsToPoints = new List<EquipmentToPointLink>
        {
            new EquipmentToPointLink
            {
                EquipmentId = Constants.EquipmentId1,
                PointEntityId = Constants.PointId2,
            },

            new EquipmentToPointLink
            {
                EquipmentId = Constants.EquipmentId1,
                PointEntityId = Constants.PointId1,
            },
            new EquipmentToPointLink
            {
                EquipmentId = Constants.EquipmentId10ForValidation,
                PointEntityId = Constants.PointId5ForValidation,
            },
            new EquipmentToPointLink
            {
                EquipmentId = Constants.EquipmentId10ForValidation,
                PointEntityId = Constants.PointId6ForValidation,
            },
            new EquipmentToPointLink
            {
                EquipmentId = Constants.EquipmentId10ForValidation,
                PointEntityId = Constants.PointId7ForValidation
            },
        };
    }
}
