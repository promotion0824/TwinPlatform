using System.Linq;
using Xunit;
using System.Collections.Generic;
using AutoFixture.Xunit2;
using PlatformPortalXL.Dto;
using FluentAssertions;
using Willow.Batch;

namespace Willow.PlatformPortal.XL.UnitTests
{
    public class SortSpecificationDtoExtensions
	{
        public SortSpecificationDtoExtensions()
        {
        }

        [Theory]
		[AutoData]
        public void SortSpecificationDto_ApplyTo(List<InsightSimpleDto> items)
        {
			var specs = new SortSpecificationDto[] { new SortSpecificationDto() { Field = "OccurredDate", Sort = "desc" } };

            items.SortBy(specs).Should().BeEquivalentTo(items.OrderByDescending(x => x.OccurredDate));
        }

		[Theory]
		[AutoData]
		public void SortSpecificationDto_ThenTo(List<InsightSimpleDto> items)
		{
			var specs = new SortSpecificationDto[] {
				new SortSpecificationDto() { Field = "OccurredDate", Sort = "desc" },
				new SortSpecificationDto() { Field = "SequenceNumber", Sort = "asc" }
			};

            items.SortBy(specs).Should().BeEquivalentTo(items
				.OrderByDescending(x => x.OccurredDate)
				.ThenBy(x => x.SequenceNumber));
		}
	}
}
