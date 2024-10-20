using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Willow.Common;
using Willow.Data.Configs;
using Xunit;

namespace Willow.Data.UnitTests;

public class StaleCacheTests
{
    [Fact]
    public async Task ErrorCreatingCacheItem_ReturnsStaleItem()
    {
        var startTime = new DateTimeOffset(2024, 1, 1, 10, 0, 0, TimeSpan.Zero);
        var logger = new Mock<ILogger<StaleCache>>();
        const string key = "this-is-a-key";

        // Two services require mocking of time, MemoryCache uses ISystemClock and StaleCache uses IDateTimeService.
        var systemClock = new Mock<ISystemClock>();
        systemClock.Setup(s => s.UtcNow).Returns(() => startTime);
        var dateTimeService = new Mock<IDateTimeService>();
        dateTimeService.Setup(s => s.UtcNow).Returns(() => systemClock.Object.UtcNow.UtcDateTime);

        var memoryCache = new MemoryCache(new MemoryCacheOptions { Clock = systemClock.Object });

        var sut = new StaleCache(
            memoryCache,
            Options.Create(new StaleCacheOptions{ ExtensionTime = TimeSpan.FromMinutes(25) }),
            dateTimeService.Object,
            logger.Object);

        var initialItem = await sut.GetOrCreateAsync(
            key,
            async () => await Task.FromResult("first"),
            new MemoryCacheEntryOptions{ AbsoluteExpiration = systemClock.Object.UtcNow.Add(TimeSpan.FromMinutes(5)) });
        initialItem.Should().Be("first");

        // Jump to where the cached item has expired but the stale item is still valid
        systemClock.Setup(s => s.UtcNow).Returns(() => startTime.AddMinutes(10));

        // Act
        var staleItem = await sut.GetOrCreateAsync(key, async () =>
        {
            var result = "second";

            if (result == "second")
            {
                throw new Exception();
            }

            return await Task.FromResult(result);
        }, new MemoryCacheEntryOptions{ AbsoluteExpiration = systemClock.Object.UtcNow.Add(TimeSpan.FromMinutes(5)) });

        staleItem.Should().Be("first");
    }

    [Fact]
    public async Task CreatingCacheItemReturnsFreshItem_WithAbsoluteExpiration()
    {
        var startTime = new DateTimeOffset(2024, 1, 1, 10, 0, 0, TimeSpan.Zero);

        var logger = new Mock<ILogger<StaleCache>>();
        const string key = "this-is-a-key";

        // Two services require mocking of time, MemoryCache uses ISystemClock and StaleCache uses IDateTimeService.
        var systemClock = new Mock<ISystemClock>();
        systemClock.Setup(s => s.UtcNow).Returns(() => startTime);
        var dateTimeService = new Mock<IDateTimeService>();
        dateTimeService.Setup(s => s.UtcNow).Returns(() => systemClock.Object.UtcNow.UtcDateTime);

        var memoryCache = new MemoryCache(new MemoryCacheOptions { Clock = systemClock.Object });

        var sut = new StaleCache(
            memoryCache,
            Options.Create(new StaleCacheOptions{ ExtensionTime = TimeSpan.FromMinutes(25) }),
            dateTimeService.Object,
            logger.Object);

        var initialItem = await sut.GetOrCreateAsync(
            key,
            async () => await Task.FromResult("first"),
            new MemoryCacheEntryOptions{ AbsoluteExpiration = systemClock.Object.UtcNow.Add(TimeSpan.FromMinutes(5)) });
        initialItem.Should().Be("first");

        // Jump to where the cached item has expired but the stale item is still valid
        systemClock.Setup(s => s.UtcNow).Returns(() => startTime.AddMinutes(10));

        // Act
        var updatedItem = await sut.GetOrCreateAsync(
            key,
            async () => await Task.FromResult("second"),
            new MemoryCacheEntryOptions{ AbsoluteExpiration = systemClock.Object.UtcNow.Add(TimeSpan.FromMinutes(5)) });
        updatedItem.Should().Be("second");
    }

    [Fact]
    public async Task CreatingCacheItemReturnsFreshItem_WithAbsoluteExpirationRelativeToNow()
    {
        var startTime = new DateTimeOffset(2024, 1, 1, 10, 0, 0, TimeSpan.Zero);

        var logger = new Mock<ILogger<StaleCache>>();
        const string key = "this-is-a-key";

        // Two services require mocking of time, MemoryCache uses ISystemClock and StaleCache uses IDateTimeService.
        var systemClock = new Mock<ISystemClock>();
        systemClock.Setup(s => s.UtcNow).Returns(() => startTime);
        var dateTimeService = new Mock<IDateTimeService>();
        dateTimeService.Setup(s => s.UtcNow).Returns(() => systemClock.Object.UtcNow.UtcDateTime);

        var memoryCache = new MemoryCache(new MemoryCacheOptions { Clock = systemClock.Object });

        var sut = new StaleCache(
            memoryCache,
            Options.Create(new StaleCacheOptions{ ExtensionTime = TimeSpan.FromMinutes(25) }),
            dateTimeService.Object,
            logger.Object);

        var initialItem = await sut.GetOrCreateAsync(
            key,
            async () => await Task.FromResult("first"),
            new MemoryCacheEntryOptions{ AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) });
        initialItem.Should().Be("first");

        // Jump to where the cached item has expired but the stale item is still valid
        systemClock.Setup(s => s.UtcNow).Returns(() => startTime.AddMinutes(10));

        // Act
        var updatedItem = await sut.GetOrCreateAsync(
            key,
            async () => await Task.FromResult("second"),
            new MemoryCacheEntryOptions{ AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) });
        updatedItem.Should().Be("second");
    }
}
