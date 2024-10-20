using System.Linq;
using System.Collections.Generic;
using FluentAssertions;
using DigitalTwinCore.Models;
using DigitalTwinCore.Services.Cacheless;
using Xunit;


namespace DigitalTwinCore.Test.Services.CachelessAdtServiceTests
{
    public class CachelessAdtServiceTests
    {
        private Twin CreateTwin(string name) {
            return new () {
                CustomProperties = new Dictionary<string, object> {
                    { "name", name }
                }
            };
        }

        [Fact]
        public void SortTreeNodes_With_Numbers()
        {
            var result = CachelessAdtService.SortTreeNodes(new List<NestedTwin>()
            {
                new NestedTwin(CreateTwin("Region 1")),
                new NestedTwin(CreateTwin("Region 30")),
                new NestedTwin(CreateTwin("Region 4"))
            });

            result.Select(n => n.Twin.GetStringProperty("name")).Should().BeEquivalentTo(
                new[] { "Region 1", "Region 4", "Region 30" },
                options => options.WithStrictOrdering()
            );
        }

        [Fact]
        public void SortTreeNodes_Without_Numbers()
        {
            var result = CachelessAdtService.SortTreeNodes(new List<NestedTwin>()
            {
                new NestedTwin(CreateTwin("")),
                new NestedTwin(CreateTwin("Zebra")),
                new NestedTwin(CreateTwin("Two")),
                new NestedTwin(CreateTwin("One")),
                new NestedTwin(CreateTwin("X X X")),
            });

            result.Select(n => n.Twin.GetStringProperty("name")).Should().BeEquivalentTo(
                new[] { "", "One", "Two", "X X X", "Zebra" },
                options => options.WithStrictOrdering()
            );
        }

        [Fact]
        public void SortTreeNodes_With_NullTwinNames()
        {
            var result = CachelessAdtService.SortTreeNodes(new List<NestedTwin>()
            {
                new NestedTwin(CreateTwin(null)),
                new NestedTwin(CreateTwin("Region 1")),
                new NestedTwin(CreateTwin("Region 30")),
                new NestedTwin(CreateTwin("Region 4")),
                new NestedTwin(CreateTwin(null)),
                new NestedTwin(CreateTwin("")),
                new NestedTwin(CreateTwin("ZZZ")),
                new NestedTwin(CreateTwin("Aa")),
                new NestedTwin(CreateTwin(" ")),

            });
            result.Select(n => n.Twin.GetStringProperty("name")).Should().BeEquivalentTo(
                new[] { null, null, "", " ", "Aa", "Region 1", "Region 4", "Region 30", "ZZZ" },
                options => options.WithStrictOrdering()
            );
        }

        [Fact]
        public void SortTreeNodes_Multiple_Levels()
        {
            var result = CachelessAdtService.SortTreeNodes(new List<NestedTwin>()
            {
                new NestedTwin(
                    CreateTwin("Region 1"),
                    new List<NestedTwin> {
                        new NestedTwin(CreateTwin("Building 1")),
                        new NestedTwin(CreateTwin("Building 30")),
                        new NestedTwin(CreateTwin("Building 4")),
                    }
                )
            });
            result[0].Children.Select(n => n.Twin.GetStringProperty("name")).Should().BeEquivalentTo(
                new[] { "Building 1", "Building 4", "Building 30" },
                options => options.WithStrictOrdering()
            );
        }

    }
}
