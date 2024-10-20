using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

using Willow.DataValidation;

using PlatformPortalXL;
using PlatformPortalXL.Features.Workflow;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Features.Inspection;
using PlatformPortalXL.Models;

namespace Willow.PlatformPortal.XL.UnitTests
{
    public class AssetCategoryExtensionsTests
    {
        public AssetCategoryExtensionsTests()
        {
        }

        [Fact]
        public void AssetCategory_GetAssets_single()
        {
            var categoryId = Guid.NewGuid();
            var categories = new List<AssetCategory>
            { 
                new AssetCategory
                {
                    Id = categoryId,
                    Assets = new List<Asset>
                    {
                        new Asset
                        {
                            Name = "bob",
                            CategoryId = categoryId
                        }
                    }
                }
            };

            var result = categories.GetAssets(null, true);

            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public void AssetCategory_GetAssets_single_byCategory()
        {
            var categoryId = Guid.NewGuid();
            var categories = new List<AssetCategory>
            { 
                new AssetCategory
                {
                    Id = categoryId,
                    Assets = new List<Asset>
                    {
                        new Asset
                        {
                            Name = "bob",
                            CategoryId = categoryId
                        }
                    }
                }
            };

            var result = categories.GetAssets(categoryId, false);

            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public void AssetCategory_GetAssets_none()
        {
            var categoryId1 = Guid.NewGuid();
            var categoryId2 = Guid.NewGuid();
            var categories = new List<AssetCategory>
            { 
                new AssetCategory
                {
                    Id = categoryId1,
                    Assets = new List<Asset>
                    {
                        new Asset
                        {
                            Name = "bob",
                            CategoryId = categoryId1
                        }
                    }
                }
            };

            var result = categories.GetAssets(categoryId2, false);

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Theory]
        [InlineData(false, 1)]
        [InlineData(true, 5)]
        public void AssetCategory_GetAssets_multiple_tree_byCategory(bool subCategories, int numAssets)
        {
            var result = _categories.GetAssets(_categoryId1, subCategories);

            Assert.NotNull(result);
            Assert.Equal(numAssets, result.Count());
        }

        [Theory]
        [InlineData(false, 2)]
        [InlineData(true, 7)]
        public void AssetCategory_GetAssets_multiple_tree_all(bool subCategories, int numAssets)
        {
            var result = _categories.GetAssets(null, subCategories);

            Assert.NotNull(result);
            Assert.Equal(numAssets, result.Count());
        }

        [Theory]
        [InlineData(false, 1)]
        [InlineData(true, 3)]
        public void AssetCategory_GetAssets_multiple_byCategory_embedded(bool subCategories, int numAssets)
        {
            var result = _categories.GetAssets(_categoryId3, subCategories);

            Assert.NotNull(result);
            Assert.Equal(numAssets, result.Count());
        }

        #region Sample Data

        private static Guid _categoryId1 = Guid.NewGuid();
        private static Guid _categoryId2 = Guid.NewGuid();
        private static Guid _categoryId3 = Guid.NewGuid();
        private static Guid _categoryId4 = Guid.NewGuid();
        private static Guid _categoryId5 = Guid.NewGuid();
        private static Guid _categoryId6 = Guid.NewGuid();
        private static Guid _categoryId7 = Guid.NewGuid();
        private static Guid _categoryId8 = Guid.NewGuid();

        private static List<AssetCategory> _categories  = new List<AssetCategory>
        { 
            new AssetCategory
            {
                Id = _categoryId1,
                Assets = new List<Asset>
                {
                    new Asset
                    {
                        Name = "bob",
                        CategoryId = _categoryId1
                    }
                },

                Categories = new List<AssetCategory>
                {
                    new AssetCategory
                    {
                        Id = _categoryId2,
                        Assets = new List<Asset>
                        {
                            new Asset
                            {
                                Name = "fred",
                                CategoryId = _categoryId2
                            }
                        }
                    },
                    new AssetCategory
                    {
                        Id = _categoryId3,
                        Assets = new List<Asset>
                        {
                            new Asset
                            {
                                Name = "fred",
                                CategoryId = _categoryId3
                            }
                        },

                        Categories = new List<AssetCategory>
                        {
                            new AssetCategory
                            {
                                Id = _categoryId4,
                                Assets = new List<Asset>
                                {
                                    new Asset
                                    {
                                        Name = "wilma",
                                        CategoryId = _categoryId4
                                    }
                                }
                            },
                            new AssetCategory
                            {
                                Id = _categoryId5,
                                Assets = new List<Asset>
                                {
                                    new Asset
                                    {
                                        Name = "barney",
                                        CategoryId = _categoryId5
                                    }
                                }
                            }
                        }
                    }
                }
            },
            new AssetCategory
            {
                Id = _categoryId6,
                Assets = new List<Asset>
                {
                    new Asset
                    {
                        Name = "babmbam",
                        CategoryId = _categoryId6
                    }
                },

                Categories = new List<AssetCategory>
                {
                    new AssetCategory
                    {
                        Id = _categoryId7,
                        Assets = new List<Asset>
                        {
                            new Asset
                            {
                                Name = "dino",
                                CategoryId = _categoryId7
                            }
                        }
                    }
                }
            }
        };

        #endregion
    }
}
