using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using DigitalTwinCore.Constants;
using DigitalTwinCore.Models;
using DigitalTwinCore.Services;
using DigitalTwinCore.Test.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Snapshooter.Xunit;
using Xunit;
using Task = System.Threading.Tasks.Task;

namespace DigitalTwinCore.Test.Services.DigitalTwinModelParser
{
    public class DigitalTwinModelParserTests
    {
        [Trait("Category", "Performance")]
        [Theory(Skip = "Testing v3")]
        [InlineData("wil-prd-lda-msft-eu22-adt-models.json")] // avg 16000-17000 ms pre perf change
        public async Task LoadsGetInterfaceDescendantsInReasonableTime(string modelFileName)
        {
            var modelParser =
                await DigitalTwinCore.Services.DigitalTwinModelParser.CreateAsync(FileHelper.LoadFile<List<Model>>(modelFileName),
                    new Mock<ILogger<IDigitalTwinService>>().Object);

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var runs = Enumerable.Range(0, 100).Select(n =>
            {
                var result = modelParser.GetInterfaceDescendants(new[] { WillowInc.PointModelId });
                Assert.NotEmpty(result);

                return result.Count;
            }).ToList();

            runs.Should().NotBeEmpty();

            stopwatch.Stop();
            var stopwatchElapsed = stopwatch.Elapsed.TotalMilliseconds;

            // Most cases on a decent CPU this should be under 200ms but give some overhead for CI/CD Runners
            // Actual benchmarks should be  run via the benchmark project this is just an indication of order of magnitute changes
            stopwatchElapsed.Should().BeLessOrEqualTo(5000); 
        }

        [Trait("Category", "Snapshot")]
        [Theory(Skip = "Testing v3")]
        [InlineData("wil-prd-lda-msft-eu22-adt-models.json")]
        public async Task GetInterfaceDescendantsMatchesSnapshotsForIds(string modelFileName)
        {
            var modelParser =
                await DigitalTwinCore.Services.DigitalTwinModelParser.CreateAsync(FileHelper.LoadFile<List<Model>>(modelFileName),
                    new Mock<ILogger<IDigitalTwinService>>().Object);

            var result = modelParser.GetInterfaceDescendants(new[] { WillowInc.PointModelId });

            result.Should().NotBeEmpty();
            result.Keys.OrderBy(i => i)
                .MatchSnapshot(
                    $"{nameof(DigitalTwinModelParserTests)}{nameof(GetInterfaceDescendantsMatchesSnapshotsForIds)}.{modelFileName}");
        }
    }
}