using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Xunit;
using Moq;

using Willow.KPI.Repository;
using Willow.KPI.Service;
using System.Globalization;

namespace KPI.API.UnitTests
{
    [Trait("KPI", "KPIAPI")]
    public class KPIAPITests
    {
        [Fact]
        public async Task KPIAPI_Get()
        {
            var repo   = new Mock<IQueryRepository>();
            var portfolioId = Guid.NewGuid();
            var siteId = Guid.NewGuid();
            var data = new List<IEnumerable<object>> { $"{portfolioId.ToString()},,Energy Score,05/19/2021,.42,%".Split(","),
                                                         $"{portfolioId.ToString()},,Energy Score,05/17/2021,.48,%".Split(","),
                                                         $"{portfolioId.ToString()},,Energy Score,05/18/2021,.49,%".Split(","),
                                                         $"{portfolioId.ToString()},,Energy Score,05/02/2021,.44,%".Split(","),
                                                         $"{portfolioId.ToString()},,kW/m2,05/19/2021,2141.43,%".Split(","),
                                                         $"{portfolioId.ToString()},,kW/m2,05/17/2021,1933.34,%".Split(","),
                                                         $"{portfolioId.ToString()},,kW/m2,05/02/2021,1578.23,%".Split(","),
                                                         $"{portfolioId.ToString()},,kW/m2,05/18/2021,2467.08,%".Split(",") };

            repo.Setup( r=> r.Query(It.IsAny<string>(), It.IsAny<object[]>())).ReturnsAsync( data.Select( d=> { var lst = d.ToList(); return new List<object> { lst[0], lst[1], lst[2], DateTime.ParseExact(lst[3].ToString(), "MM/dd/yyyy", CultureInfo.InvariantCulture), lst[4], lst[5] }; } ));

            var svc    = new KPIAPI(repo.Object, "bob");
            var result = (await svc.GetByMetric(portfolioId, "test", new { SiteIds = "", StartDate = "fred", EndDate = "wilma" } , true)).ToList();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(4, result[0].Values.Count);
            Assert.Equal(4, result[1].Values.Count);
            Assert.Equal("Energy Score", result[0].Name);
            Assert.Equal("kW/m2", result[1].Name);
            Assert.Equal(DateTime.Parse("2021-05-02T00:00:00.0000000"), result[0].Values[0].XValue);
            Assert.Equal(".44", result[0].Values[0].YValue);
            Assert.Equal(DateTime.Parse("2021-05-19T00:00:00.0000000"), result[1].Values[3].XValue);
            Assert.Equal("2141.43", result[1].Values[3].YValue);

            repo.Verify( r=> r.Query("SELECT * FROM TABLE(bob.get_test_data_udtf(?,?,?,?))", It.IsAny<object[]>()), Times.Once);
        }

		[Fact]
		public async Task KPIAPI_GetWithRangeFilters()
		{
			var repo = new Mock<IQueryRepository>();
			var portfolioId = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			var data = new List<IEnumerable<object>> { $"{portfolioId.ToString()},,Energy Score,05/19/2021,.42,%".Split(","),
														 $"{portfolioId.ToString()},,Energy Score,05/17/2021,.48,%".Split(","),
														 $"{portfolioId.ToString()},,Energy Score,05/18/2021,.49,%".Split(","),
														 $"{portfolioId.ToString()},,Energy Score,05/02/2021,.44,%".Split(","),
														 $"{portfolioId.ToString()},,kW/m2,05/19/2021,2141.43,%".Split(","),
														 $"{portfolioId.ToString()},,kW/m2,05/17/2021,1933.34,%".Split(","),
														 $"{portfolioId.ToString()},,kW/m2,05/02/2021,1578.23,%".Split(","),
														 $"{portfolioId.ToString()},,kW/m2,05/18/2021,2467.08,%".Split(",") };

			repo.Setup(r => r.Query(It.IsAny<string>(), It.IsAny<object[]>())).ReturnsAsync(data.Select(d => { var lst = d.ToList(); return new List<object> { lst[0], lst[1], lst[2], DateTime.ParseExact(lst[3].ToString(), "MM/dd/yyyy", CultureInfo.InvariantCulture), lst[4], lst[5] }; }));

			var svc = new KPIAPI(repo.Object, "bob");
			var result = (await svc.GetByMetric(portfolioId, "test", new { SiteIds = "", StartDate = "fred", EndDate = "wilma", isWeekDay = true, isBusinessHour = false }, true)).ToList();

			Assert.NotNull(result);
			Assert.Equal(2, result.Count);
			Assert.Equal(4, result[0].Values.Count);
			Assert.Equal(4, result[1].Values.Count);
			Assert.Equal("Energy Score", result[0].Name);
			Assert.Equal("kW/m2", result[1].Name);
			Assert.Equal(DateTime.Parse("2021-05-02T00:00:00.0000000"), result[0].Values[0].XValue);
			Assert.Equal(".44", result[0].Values[0].YValue);
			Assert.Equal(DateTime.Parse("2021-05-19T00:00:00.0000000"), result[1].Values[3].XValue);
			Assert.Equal("2141.43", result[1].Values[3].YValue);

			repo.Verify(r => r.Query("SELECT * FROM TABLE(bob.get_test_data_udtf(?,?,?,?,?,?))", It.IsAny<object[]>()), Times.Once);
		}

		[Fact]
		public async Task KPIAPI_GetWithDayRangeFilter()
		{
			var repo = new Mock<IQueryRepository>();
			var portfolioId = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			var data = new List<IEnumerable<object>> { $"{portfolioId.ToString()},,Energy Score,05/19/2021,.42,%".Split(","),
														 $"{portfolioId.ToString()},,Energy Score,05/17/2021,.48,%".Split(","),
														 $"{portfolioId.ToString()},,Energy Score,05/18/2021,.49,%".Split(","),
														 $"{portfolioId.ToString()},,Energy Score,05/02/2021,.44,%".Split(","),
														 $"{portfolioId.ToString()},,kW/m2,05/19/2021,2141.43,%".Split(","),
														 $"{portfolioId.ToString()},,kW/m2,05/17/2021,1933.34,%".Split(","),
														 $"{portfolioId.ToString()},,kW/m2,05/02/2021,1578.23,%".Split(","),
														 $"{portfolioId.ToString()},,kW/m2,05/18/2021,2467.08,%".Split(",") };

			repo.Setup(r => r.Query(It.IsAny<string>(), It.IsAny<object[]>())).ReturnsAsync(data.Select(d => { var lst = d.ToList(); return new List<object> { lst[0], lst[1], lst[2], DateTime.ParseExact(lst[3].ToString(), "MM/dd/yyyy", CultureInfo.InvariantCulture), lst[4], lst[5] }; }));

			var svc = new KPIAPI(repo.Object, "bob");
			var result = (await svc.GetByMetric(portfolioId, "test", new { SiteIds = "", StartDate = "fred", EndDate = "wilma", isWeekDay = true }, true)).ToList();

			Assert.NotNull(result);
			Assert.Equal(2, result.Count);
			Assert.Equal(4, result[0].Values.Count);
			Assert.Equal(4, result[1].Values.Count);
			Assert.Equal("Energy Score", result[0].Name);
			Assert.Equal("kW/m2", result[1].Name);
			Assert.Equal(DateTime.Parse("2021-05-02T00:00:00.0000000"), result[0].Values[0].XValue);
			Assert.Equal(".44", result[0].Values[0].YValue);
			Assert.Equal(DateTime.Parse("2021-05-19T00:00:00.0000000"), result[1].Values[3].XValue);
			Assert.Equal("2141.43", result[1].Values[3].YValue);

			repo.Verify(r => r.Query("SELECT * FROM TABLE(bob.get_test_data_udtf(?,?,?,?,?))", It.IsAny<object[]>()), Times.Once);
		}

        [Fact]
        public async Task KPIAPI_GetWithGroupByDate()
        {
            var repo = new Mock<IQueryRepository>();
            var portfolioId = Guid.NewGuid();
            var siteId = Guid.NewGuid();

            //[portfolioId, siteId, data_point_name, x_data, y_data, y_data_uom]
            var data = new List<IEnumerable<object>> { $"{portfolioId.ToString()},,EnergyScore_LastValue,05/19/2021,.42,%".Split(","),
                                                         $"{portfolioId.ToString()},,EnergyScore_LastValue,05/17/2021,.48,%".Split(","),
                                                         $"{portfolioId.ToString()},,EnergyScore_LastValue,05/18/2021,.49,%".Split(","),
                                                         $"{portfolioId.ToString()},,EnergyScore_LastValue,05/02/2021,.44,%".Split(","),
                                                         $"{portfolioId.ToString()},,ComfortScore_LastValue,05/19/2021,2141.43,%".Split(","),
                                                         $"{portfolioId.ToString()},,ComfortScore_LastValue,05/17/2021,1933.34,%".Split(","),
                                                         $"{portfolioId.ToString()},,ComfortScore_LastValue,05/02/2021,1578.23,%".Split(","),
                                                         $"{portfolioId.ToString()},,ComfortScore_LastValue,05/18/2021,2467.08,%".Split(",")
            };

            repo.Setup(r => r.Query(It.IsAny<string>(), It.IsAny<object[]>())).ReturnsAsync(data.Select(d => { var lst = d.ToList(); return new List<object> { lst[0], lst[1], lst[2], DateTime.ParseExact(lst[3].ToString(), "MM/dd/yyyy", CultureInfo.InvariantCulture), lst[4], lst[5] }; }));

            var svc = new KPIAPI(repo.Object, "bob");
            var filters = new { SiteIds = "", StartDate = "05/02/2021", EndDate = "05/19/2021", isWeekDay = true, isBusinessHour = false, groupBy = "date" };
            var result = (await svc.GetByMetric(portfolioId, "test", filters, true)).ToList();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(4, result[0].Values.Count);
            Assert.Equal(4, result[1].Values.Count);
            Assert.Equal("EnergyScore_LastValue", result[0].Name);
            Assert.Equal("ComfortScore_LastValue", result[1].Name);
            Assert.Equal(DateTime.Parse("2021-05-02T00:00:00.0000000"), result[0].Values[0].XValue);
            Assert.Equal(".44", result[0].Values[0].YValue);
            Assert.Equal(DateTime.Parse("2021-05-19T00:00:00.0000000"), result[1].Values[3].XValue);
            Assert.Equal("2141.43", result[1].Values[3].YValue);

            repo.Verify(r => r.Query("SELECT * FROM TABLE(bob.get_test_data_udtf(?,?,?,?,?,?,?))", It.IsAny<object[]>()), Times.Once);
        }
    }
}
