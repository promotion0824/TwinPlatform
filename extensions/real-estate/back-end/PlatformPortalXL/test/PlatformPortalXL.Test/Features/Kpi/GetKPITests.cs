using AutoFixture;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Willow.Platform.Models;
using Xunit.Abstractions;
using Xunit;
using Willow.Tests.Infrastructure;
using System.Net.Http;
using FluentAssertions;

namespace PlatformPortalXL.Test.Features.Kpi
{
    [Trait("PPXL", "KPI")]
	public class KpiTests : BaseInMemoryTest
	{
		public KpiTests(ITestOutputHelper output) : base(output)
		{
		}

		//[Fact]
		//public async Task BuildingDataExists_GetBuildingDataByTwinID_ReturnsPerformanceScores()
		//{

            // /twins/{twinId}/kpi/building_data

            // Arrange
			//var customerId = Guid.Parse("35a800f0-67a4-4e24-ba63-cb7e0d485791");
   //         var spaceTwinId = "";

            // Act
            // Assert
        //    Assert.True(false);
        //}



        //[Fact]
        //public async Task SiteDataExists_GetSiteData_ReturnsPerformanceScores()
        //{
        //    // Arrange
        //    var customerId = Guid.Parse("35a800f0-67a4-4e24-ba63-cb7e0d485791");
        //    var spaceTwinId = "";

        //    // Act
        //    // Assert
        //}

	}
}
