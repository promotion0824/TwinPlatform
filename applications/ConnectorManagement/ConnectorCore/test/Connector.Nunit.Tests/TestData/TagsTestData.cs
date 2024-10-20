namespace Connector.Nunit.Tests.TestData
{
    using System;
    using System.Collections.Generic;
    using ConnectorCore.Entities;
    using ConnectorCore.Models;

    public class TagsTestData
    {
        public static List<CategoryEntity> TagCategories = new List<CategoryEntity>
        {
            new CategoryEntity
            {
                Id = Constants.TagCategoryId1,

                //ClientId = Constants.ClientIdDefault,
                Name = "Category 1",
            },
            new CategoryEntity
            {
                Id = Constants.TagCategoryId2,
                ClientId = Constants.ClientIdDefault,
                Name = "Category 2",
                ParentId = Constants.TagCategoryId1,
            },
            new CategoryEntity
            {
                Id = Constants.TagCategoryId3ToDelete,
                ClientId = Constants.ClientIdDefault,
                Name = "Category 3",
            },
            new CategoryEntity
            {
                Id = Constants.TagCategoryId4ToRemoveTags,
                ClientId = Constants.ClientIdDefault,
                Name = "Category 4",
            },
            new CategoryEntity
            {
                Id = Constants.TagCategoryId5ToAddTags,
                ClientId = Constants.ClientIdDefault,
                Name = "Category 5"
            },
        };

        public static List<TagCategoryLinkEntity> TagCategoryLinks = new List<TagCategoryLinkEntity>
        {
            new TagCategoryLinkEntity
            {
                CategoryId = Constants.TagCategoryId1,
                TagId = Constants.TagForCategoryId1,
            },
            new TagCategoryLinkEntity
            {
                CategoryId = Constants.TagCategoryId1,
                TagId = Constants.TagForCategoryId2,
            },
            new TagCategoryLinkEntity
            {
                CategoryId = Constants.TagCategoryId1,
                TagId = Constants.TagForCategoryId3,
            },

            new TagCategoryLinkEntity
            {
                CategoryId = Constants.TagCategoryId2,
                TagId = Constants.TagForCategoryId1,
            },
            new TagCategoryLinkEntity
            {
                CategoryId = Constants.TagCategoryId2,
                TagId = Constants.TagForCategoryId2,
            },

            new TagCategoryLinkEntity
            {
                CategoryId = Constants.TagCategoryId3ToDelete,
                TagId = Constants.TagForCategoryId1,
            },
            new TagCategoryLinkEntity
            {
                CategoryId = Constants.TagCategoryId3ToDelete,
                TagId = Constants.TagForCategoryId2,
            },
            new TagCategoryLinkEntity
            {
                CategoryId = Constants.TagCategoryId3ToDelete,
                TagId = Constants.TagForCategoryId3,
            },

            new TagCategoryLinkEntity
            {
                CategoryId = Constants.TagCategoryId4ToRemoveTags,
                TagId = Constants.TagForCategoryId1,
            },
            new TagCategoryLinkEntity
            {
                CategoryId = Constants.TagCategoryId4ToRemoveTags,
                TagId = Constants.TagForCategoryId2,
            },
        };

        public static List<TagEntity> Tags = new List<TagEntity>
        {
            new TagEntity
            {
                Id = Constants.TagForCategoryId1,
                Name = "AttachedTag1",
                Description = "AttachedTag1",
                ClientId = Constants.ClientIdDefault,
            },
            new TagEntity
            {
                Id = Constants.TagForCategoryId2,
                Name = "AttachedTag2",
                Description = "AttachedTag2",
                ClientId = Constants.ClientIdDefault,
            }, new TagEntity
            {
                Id = Constants.TagForCategoryId3,
                Name = "AttachedTag3",
                Description = "AttachedTag3",
                ClientId = Constants.ClientIdDefault,
            },
            new TagEntity
            {
                Id = Constants.TagId1,
                Name = "Tag1",
                Description = "Tag1",

                //CategoryId = Guid.Parse("81296973-c936-4804-a9a0-a447508d9bcf"),
            },
            new TagEntity
            {
                Id = Constants.TagId2,
                Name = "Tag2",
                Description = "Tag2",

                //CategoryId = Guid.Parse("81296973-c936-4804-a9a0-a447508d9bcf"),
            },
            new TagEntity
            {
                Id = Constants.TagId3,
                Name = "Tag3 to be deleted",
                Description = "Tag3 to be deleted",

                //CategoryId = Guid.Parse("51f66fe2-0a46-485e-afee-3e546baaf4ed"),
            },
            new TagEntity
            {
                Id = Constants.TagId4,
                Name = "Tag4",
                Description = "Tag4",

                //CategoryId = Guid.Parse("57afd6d4-1f25-4753-9419-5dffd698c24c"),
            },
            new TagEntity
            {
                Id = Constants.TagId5ToUpdate,
                Name = "Tag5 to be updated",
                Description = "Tag5 to be updated",
            },
            new TagEntity
            {
                Id = Constants.TagId6,
                Name = "Tag6 to be deleted",
                Description = "Tag6 to be deleted",
            },
            new TagEntity
            {
                Id = Constants.TagId7,
                Name = "Tag7 to be deleted",
                Description = "Tag7 to be deleted",
            },
            new TagEntity
            {
                Id = Constants.TagId8,
                Name = "Tag8 to be deleted",
                Description = "Tag8 to be deleted",
            },
            new TagEntity
            {
                Id = Guid.Parse("cbe55231-7ddd-4cfa-85e0-de5ea5646c01"),
                Name = "Tag replace 1 point",
                Description = "Tag replace 1 point",

                //CategoryId = Guid.Parse("713fbc79-d2d9-4457-aa73-34ae33bfc796"),
            }, new TagEntity
            {
                Id = Guid.Parse("73bd8b3d-f640-466c-8e27-d89460e1bebb"),
                Name = "Tag replace 2 point",
                Description = "Tag replace 2 point",

                //CategoryId = Guid.Parse("713fbc79-d2d9-4457-aa73-34ae33bfc796"),
            }, new TagEntity
            {
                Id = Guid.Parse("858f4d08-3eaa-4799-b9aa-888133ed203a"),
                Name = "Tag replace 3 point",
                Description = "Tag replace 3 point",

                //CategoryId = Guid.Parse("713fbc79-d2d9-4457-aa73-34ae33bfc796"),
            },
            new TagEntity
            {
                Id = Constants.TagIdToAddToPoint1,
                Name = "Tag to add to Point 1",
                Description = "Tag to add to Point 1",
            },
            new TagEntity
            {
                Id = Constants.TagIdToAddToPoint2,
                Name = "Tag to add to Point 2",
                Description = "Tag to add to Point 2",
            },
            new TagEntity
            {
                Id = Constants.TagIdToAddToEquipment1,
                Name = "Tag to add to Equipment 1",
                Description = "Tag to add to Equipment 1",
            },
            new TagEntity
            {
                Id = Constants.TagIdToAddToEquipment2,
                Name = "Tag to add to Equipment 2",
                Description = "Tag to add to Equipment 2",
            },
            new TagEntity
            {
                Id = Constants.TagId1ForFeature,
                Name = "Tag 1 with Feature",
                Description = "Tag 1 with Feature",
                Feature = Constants.TagFeature1,
            },
            new TagEntity
            {
                Id = Constants.TagId2ForFeature,
                Name = "Tag 2 with Feature",
                Description = "Tag 2 with Feature",
                Feature = Constants.TagFeature1
            },
        };

        public static List<PointToTagLink> PointToTagLinks = new List<PointToTagLink>
        {
            new PointToTagLink
            {
                PointId = Constants.PointId1,
                TagId = Constants.TagId4,
            },
            new PointToTagLink
            {
                PointId = Constants.PointId2,
                TagId = Constants.TagId1
            },
        };

        public static List<EquipmentToTagLink> EquipmentToTagLinks = new List<EquipmentToTagLink>
        {
            new EquipmentToTagLink
            {
                EquipmentId = Constants.EquipmentId1,
                TagId = Constants.TagId4,
            },

            new EquipmentToTagLink
            {
                EquipmentId = Constants.EquipmentId2,
                TagId = Constants.TagForCategoryId1,
            },
            new EquipmentToTagLink
            {
                EquipmentId = Constants.EquipmentId2,
                TagId = Constants.TagForCategoryId2,
            },
            new EquipmentToTagLink
            {
                EquipmentId = Constants.EquipmentId2,
                TagId = Constants.TagForCategoryId3
            },
        };
    }
}
