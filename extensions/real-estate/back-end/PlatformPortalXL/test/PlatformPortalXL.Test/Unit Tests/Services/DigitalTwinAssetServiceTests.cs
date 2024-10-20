using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Caching.Memory;

using Xunit;
using Moq;
using PlatformPortalXL.Models;
using PlatformPortalXL.ServicesApi.DigitalTwinApi;
using PlatformPortalXL.Services.Assets;

namespace Willow.PlatformPortal.XL.UnitTests
{
    public class DigitalTwinAssetServiceTests
    {
        private readonly IMemoryCache                 _memoryCache = new MemoryCache(new MemoryCacheOptions());
        private readonly Mock<IDigitalTwinApiService> _digitalTwinApiService = new Mock<IDigitalTwinApiService>();
        private readonly DigitalTwinAssetService      _service;

        public DigitalTwinAssetServiceTests()
        {
            _service = new DigitalTwinAssetService(_memoryCache, _digitalTwinApiService.Object);
        }

        [Fact]
        public async Task DigitalTwinAssetService_GetAssetsAsync_single()
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

            _digitalTwinApiService.Setup( s=> s.GetAssetTreeAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<bool>(), It.IsAny<List<string>>())).ReturnsAsync(categories);

            var result = await _service.GetAssetsAsync(Guid.NewGuid(), categoryId, null, null, false, null);

            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public async Task DigitalTwinAssetService_GetAssets_single_byCategory()
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

            _digitalTwinApiService.Setup( s=> s.GetAssetTreeAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<bool>(), It.IsAny<List<string>>())).ReturnsAsync(categories);

            var result = await _service.GetAssetsAsync(Guid.NewGuid(), categoryId, null, null, false, null);

            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public async Task DigitalTwinAssetService_GetAssets_none()
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

            _digitalTwinApiService.Setup( s=> s.GetAssetTreeAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<bool>(), It.IsAny<List<string>>())).ReturnsAsync(categories);

            var result = await _service.GetAssetsAsync(Guid.NewGuid(), categoryId2, null, null, false, null);

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Theory]
        [InlineData(false, 1)]
        [InlineData(true, 5)]
        public async Task DigitalTwinAssetService_GetAssets_multiple_tree_byCategory(bool subCategories, int numAssets)
        {
            _digitalTwinApiService.Setup( s=> s.GetAssetTreeAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<bool>(), It.IsAny<List<string>>())).ReturnsAsync(_categories);

            var result = await _service.GetAssetsAsync(Guid.NewGuid(), _categoryId1, null, null, subCategories, null);

            Assert.NotNull(result);
            Assert.Equal(numAssets, result.Count());
        }

        [Fact]
        public async Task DigitalTwinAssetService_GetAssets_multiple_tree_all()
        {
            _digitalTwinApiService.Setup( s=> s.GetAssetTreeAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<bool>(), It.IsAny<List<string>>())).ReturnsAsync(_categories);

            var result = await _service.GetAssetsAsync(Guid.NewGuid(), null, null, null, true, null);

            Assert.NotNull(result);
            Assert.Equal(7, result.Count());
        }

        [Theory]
        [InlineData(false, 1)]
        [InlineData(true, 3)]
        public async Task DigitalTwinAssetService_GetAssets_multiple_byCategory_embedded(bool subCategories, int numAssets)
        {
            _digitalTwinApiService.Setup( s=> s.GetAssetTreeAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<bool>(), It.IsAny<List<string>>())).ReturnsAsync(_categories);

            var result = await _service.GetAssetsAsync(Guid.NewGuid(), _categoryId3, null, null, subCategories, null);

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
