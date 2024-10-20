namespace Connector.Nunit.Tests.TestData
{
    using System;
    using System.Collections.Generic;
    using ConnectorCore.Entities;

    public class SchemasTestData
    {
        public static List<SchemaEntity> Schemas = new List<SchemaEntity>
        {
            new SchemaEntity
            {
                Id = Constants.SchemaId1,
                Name = "Schema 1",
                Type = "SchemaType1",
            },

            new SchemaEntity
            {
                Id = Guid.Parse("fb2c7a16-d914-4aca-abfe-fb67548f3766"),
                Name = "Schema 2",
                Type = "SchemaType1",
            },

            new SchemaEntity
            {
                Id = Guid.Parse("cfbf5cd7-94f2-4b32-961d-a931f1253492"),
                Name = "Schema 3 NOT USED",
                Type = "SchemaType1",
            },

            new SchemaEntity
            {
                Id = Guid.Parse("107c95d5-b759-43b6-abbd-8464b9bdf529"),
                Name = "Schema To Be Deleted",
                Type = "SchemaType2",
            },

            new SchemaEntity
            {
                Id = Guid.Parse("bbe1a34c-1322-4f5f-a30b-b1f0bfab2a9a"),
                Name = "Connector configuration schema",
                Type = "SchemaType3",
            },

            new SchemaEntity
            {
                Id = Guid.Parse("e6c7ae20-2ff2-4d90-859d-89e01ebbc26c"),
                Name = "Device metadata schema",
                Type = "SchemaType4",
            },

            new SchemaEntity
            {
                Id = Guid.Parse("30a3db15-6eff-4e82-ac09-2e7b52040e54"),
                Name = "Point metadata schema",
                Type = "SchemaType5"
            },
        };
    }

    public class SchemaColumnsTestData
    {
        public static List<SchemaColumnEntity> SchemaColumns = new List<SchemaColumnEntity>
        {
            new SchemaColumnEntity()
            {
                Id = Guid.Parse("91cd8283-b7e9-4b45-bd0d-b443915ae87f"),
                DataType = "string",

                //GroupName = "Group 1",
                IsRequired = true,
                Name = "ColumnString",

                //OrderInGroup = 1,
                SchemaId = Constants.SchemaId1,
            },

            new SchemaColumnEntity()
            {
                Id = Guid.Parse("638973fb-8845-47b3-8522-59b5a8b46783"),
                DataType = "number",

                //GroupName = "Group 1",
                IsRequired = true,
                Name = "ColumnInt",

                //OrderInGroup = 2,
                SchemaId = Constants.SchemaId1,
            },

            new SchemaColumnEntity()
            {
                Id = Guid.Parse("2f9d2d28-e38a-4776-85b2-f502aeb0d0a5"),
                DataType = "string",

                //GroupName = "Group 2",
                IsRequired = false,
                Name = "Column 3",

                //OrderInGroup = 1,
                SchemaId = Guid.Parse("fb2c7a16-d914-4aca-abfe-fb67548f3766"),
            },

            new SchemaColumnEntity()
            {
                Id = Guid.Parse("c825988e-c198-431a-9d84-722a126abef2"),
                DataType = "string",

                //GroupName = "Group 2",
                IsRequired = false,
                Name = "Column to be deleted",

                //OrderInGroup = 2,
                SchemaId = Guid.Parse("fb2c7a16-d914-4aca-abfe-fb67548f3766"),
            },
            new SchemaColumnEntity()
            {
                Id = Guid.Parse("5ecd48f5-4149-4127-835c-86a74bdcf57b"),
                DataType = "string",

                //GroupName = "Group 1",
                IsRequired = true,
                Name = "Name",

                //OrderInGroup = 1,
                SchemaId = Guid.Parse("e6c7ae20-2ff2-4d90-859d-89e01ebbc26c"),
            },
            new SchemaColumnEntity()
            {
                Id = Guid.Parse("3c50ddfa-2665-433a-ba6e-a5e7267ba21f"),
                DataType = "number",

                //GroupName = "Group 1",
                IsRequired = false,
                Name = "WholeNumber",

                //OrderInGroup = 2,
                SchemaId = Guid.Parse("e6c7ae20-2ff2-4d90-859d-89e01ebbc26c"),
            },
            new SchemaColumnEntity()
            {
                Id = Guid.Parse("b44b267d-449d-41ba-ad19-93ada8b3fc84"),
                DataType = "string",

                //GroupName = "Group 1",
                IsRequired = true,
                Name = "Name",

                //OrderInGroup = 1,
                SchemaId = Guid.Parse("30a3db15-6eff-4e82-ac09-2e7b52040e54"),
            },
            new SchemaColumnEntity()
            {
                Id = Guid.Parse("bbb9cfa2-4d0d-4793-acf9-cb27a961867f"),
                DataType = "number",

                //GroupName = "Group 1",
                IsRequired = false,
                Name = "WholeNumber",

                //OrderInGroup = 2,
                SchemaId = Guid.Parse("30a3db15-6eff-4e82-ac09-2e7b52040e54"),
            },
            new SchemaColumnEntity()
            {
                Id = Guid.Parse("3a49758b-d0d8-4cbd-9505-d51afa8794e7"),
                DataType = "string",

                //GroupName = "Group 1",
                IsRequired = true,
                Name = "Name",

                //OrderInGroup = 1,
                SchemaId = Guid.Parse("bbe1a34c-1322-4f5f-a30b-b1f0bfab2a9a"),
            },
            new SchemaColumnEntity()
            {
                Id = Guid.Parse("beb10a77-5d8a-4e5f-9d44-9ccb068806f0"),
                DataType = "number",

                //GroupName = "Group 1",
                IsRequired = false,
                Name = "WholeNumber",

                //OrderInGroup = 2,
                SchemaId = Guid.Parse("bbe1a34c-1322-4f5f-a30b-b1f0bfab2a9a"),
            },
        };
    }
}
