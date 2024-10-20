using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Willow.Data;
using Willow.KPI.Repository;
using Willow.KPI.Service;
using Willow.Platform.Models;

using PlatformPortalXL.Services;
using PlatformPortalXL.Features;
using Willow.ExceptionHandling.Exceptions;
using PlatformPortalXL.Features.Pilot;
using PlatformPortalXL.ServicesApi.DigitalTwinApi;
using Microsoft.Extensions.Logging;

namespace PlatformPortalXL.Test.Unit_Tests
{
    internal class FakeKPIServiceFactory : IKPIServiceFactory
    {
        private readonly string _name;
        private readonly IReadRepository<Guid, Site> _siteRepo;
        private readonly IDigitalTwinApiService _dtApiService;
        private readonly ILogger<KPIService> _logger;

        public FakeKPIServiceFactory(string name, IReadRepository<Guid, Site> siteRepo, IDigitalTwinApiService dtApiService)
        {
            _name = name;
            _siteRepo = siteRepo ?? throw new ArgumentNullException(nameof(siteRepo));
            _dtApiService = dtApiService ?? throw new ArgumentNullException(nameof(dtApiService));
        }

        public IKPIService GetService(Guid customerId)
        {
            return new KPIService(customerId, new KPIAPI(new FakeQueryRepository(_name), "bob"), _siteRepo, _dtApiService, new MemoryCache(new MemoryCacheOptions()), 2, _logger);
        }
    }

    internal class FakeQueryRepository : IQueryRepository
    {
        private readonly string _name;

        // Twins
        // DDK - North America
        static private readonly TwinDto _twin1 = new()
        {
            Id = "WIL-101RS",
            Name = "101 Ridley Square",
            SiteId = Guid.Parse("e3ea6775-50e5-4d19-afec-b103d08658a3"),
            Metadata = new TwinMetadataDto() { ModelId = "dtmi:com:willowinc:Building;1" }
        };
        static private readonly TwinDto _twin2 = new()
        {
            Id = "WIL-Retail-007",
            Name = "Retail Store #7",
            SiteId = Guid.Parse("bbb0dd63-656e-46e7-b523-1af465d24aa9"),
            Metadata = new TwinMetadataDto() { ModelId = "dtmi:com:willowinc:Building;1" }
        };

        // DDK - Europe
        static private readonly TwinDto _twin3 = new()
        {
            Id = "WIL-220FA",
            Name = "220 Francis Avenue",
            SiteId = Guid.Parse("2bada6d2-ccd7-43dd-a42a-c8ab0873df64"),
            Metadata = new TwinMetadataDto() { ModelId = "dtmi:com:willowinc:Building;1" }
        };
        static private readonly TwinDto _twin4 = new()
        {
            Id = "WIL-104BDFD",
            Name = "104 Bedford Square",
            SiteId = Guid.Parse("a226929d-6e27-480f-b8dd-40ffbc47024c"),
            Metadata = new TwinMetadataDto() { ModelId = "dtmi:com:willowinc:Building;1" }
        };
        static private readonly TwinDto _twin5 = new()
        {
            Id = "WIL-57CM",
            Name = "Canary Wharf Underground Parking",
            SiteId = Guid.Parse("45ac7d4b-fd70-4f7c-a220-e944112159cc"),
            Metadata = new TwinMetadataDto() { ModelId = "dtmi:com:willowinc:Building;1" }
        };
        static private readonly TwinDto _twin6 = new()
        {
            Id = "Wil-CanaryWharf-Substructure",
            Name = "57 CJ Marina",
            SiteId = Guid.Parse("f46f061b-2070-4971-849e-93df84aaaf2e"),
            Metadata = new TwinMetadataDto() { ModelId = "dtmi:com:willowinc:Building;1" }
        };
        static private readonly TwinDto _twin7 = new()
        {
            Id = "WIL-CanaryWharf-JubileePark",
            Name = "Jubilee Park",
            SiteId = Guid.Parse("a598746e-66e4-497c-a04a-6a928178377a"),
            Metadata = new TwinMetadataDto() { ModelId = "dtmi:com:willowinc:Building;1" }
        };

        static private readonly TwinDto _twin_nometadata = new() { Id = "NOMETADATA", Name = "101 Ridley Square" };

        public FakeQueryRepository(string name)
        {
            _name = name;
        }

        public async Task<IEnumerable<IEnumerable<object>>> Query(string query, object[] kpiRequest = null)
        {
            if(query.Contains("trends", StringComparison.InvariantCultureIgnoreCase))
                return await OperationalTrends();

            if(query.Contains("building", StringComparison.InvariantCultureIgnoreCase))
                if (kpiRequest[kpiRequest.Length - 1].ToString().Contains("date", StringComparison.InvariantCultureIgnoreCase))
                    // Params correspond to a KPIRequest object; Last param is the GroupBy property
                    return await DatedBuildingData();
                else return await Buildings();

            if(query.Contains("kpis", StringComparison.InvariantCultureIgnoreCase))
                return await Kpis();

            if(query.Contains("overall_performance", StringComparison.InvariantCultureIgnoreCase))
                return await Overall();

            throw new NotFoundException("View not found");
        }

        private static Task<IEnumerable<IEnumerable<object>>> Overall()
        {
            var lines = new List<List<object>>();

            lines.Add(new List<object> { "Overall Performance", "", .65} );
            lines.Add(new List<object> { "Change over period", "", -.002} );
            lines.Add(new List<object> { "Yearly average", "", .82} );
            lines.Add(new List<object> { "Monthly average", "", .82} );

            IEnumerable<IEnumerable<object>> eLines = lines;

            return Task.FromResult(eLines);
        }

        private static Task<IEnumerable<IEnumerable<object>>> Kpis()
        {
            var lines = new List<List<object>>();
            var portfolioId = Guid.NewGuid().ToString();
            var siteId1  = Guid.Parse("404bd33c-a697-4027-b6a6-677e30a53d07");
            var siteId2  = Guid.Parse("a6b78f54-9875-47bc-9612-aa991cc464f3");
            var siteId3  = Guid.Parse("934638e3-4bd7-4749-bd52-bd6e47d0fbb2");
            var siteId4  = Guid.Parse("e719ac18-192b-4174-91db-b3a624f1f1a4");
            var siteId5  = Guid.Parse("092D18F5-F5CE-400F-B904-FD4F7BAF104B");
            var siteId6  = Guid.Parse("EC327557-950A-45CA-9B54-8D36752DBB6B");
            var siteId7  = Guid.Parse("180E1DCF-2F74-4D01-8019-E2D458CD7B87");
            var siteId8  = Guid.Parse("3A6D58E8-C8E0-4F30-AA58-38BAFB7937E2");
            var siteId9  = Guid.Parse("E9AE3306-D6FE-497B-BF82-1971C891525A");
            var siteId10 = Guid.Parse("C90B5874-8E9E-4AFB-994F-26E7E8EB8818");

            lines.Add(new List<object> { $"{portfolioId}", $"{siteId1}",  "Energy Cost",              "", .98, "%"} );
            lines.Add(new List<object> { $"{portfolioId}", $"{siteId2}",  "Energy Cost",              "", .9, "%"} );
            lines.Add(new List<object> { $"{portfolioId}", $"{siteId3}",  "Energy Cost",              "", .8, "%"} );
            lines.Add(new List<object> { $"{portfolioId}", $"{siteId4}",  "Energy Cost",              "", .9, "%"} );
            lines.Add(new List<object> { $"{portfolioId}", $"{siteId5}",  "Energy Cost",              "", .7, "%"} );
            lines.Add(new List<object> { $"{portfolioId}", $"{siteId6}",  "Energy Cost",              "", .5, "%"} );
            lines.Add(new List<object> { $"{portfolioId}", $"{siteId7}",  "Energy Cost",              "", .4, "%"} );
            lines.Add(new List<object> { $"{portfolioId}", $"{siteId8}",  "Energy Cost",              "", .5, "%"} );
            lines.Add(new List<object> { $"{portfolioId}", $"{siteId9}",  "Energy Cost",              "", .6, "%"} );
            lines.Add(new List<object> { $"{portfolioId}", $"{siteId10}", "Energy Cost",              "", .4, "%"} );
            lines.Add(new List<object> { $"{portfolioId}", $"{siteId1}",  "Comfort",                  "", .6, "%"} );
            lines.Add(new List<object> { $"{portfolioId}", $"{siteId2}",  "Comfort",                  "", .8, "%"} );
            lines.Add(new List<object> { $"{portfolioId}", $"{siteId3}",  "Comfort",                  "", .3, "%"} );
            lines.Add(new List<object> { $"{portfolioId}", $"{siteId4}",  "Comfort",                  "", .4, "%"} );
            lines.Add(new List<object> { $"{portfolioId}", $"{siteId5}",  "Comfort",                  "", .5, "%"} );
            lines.Add(new List<object> { $"{portfolioId}", $"{siteId6}",  "Comfort",                  "", .4, "%"} );
            lines.Add(new List<object> { $"{portfolioId}", $"{siteId7}",  "Comfort",                  "", .8, "%"} );
            lines.Add(new List<object> { $"{portfolioId}", $"{siteId8}",  "Comfort",                  "", .5, "%"} );
            lines.Add(new List<object> { $"{portfolioId}", $"{siteId9}",  "Comfort",                  "", .6, "%"} );
            lines.Add(new List<object> { $"{portfolioId}", $"{siteId10}", "Comfort",                  "", .7, "%"} );
            lines.Add(new List<object> { $"{portfolioId}", $"{siteId1}",  "Equipment Lifecycle Risk", "", .8, "%"} );
            lines.Add(new List<object> { $"{portfolioId}", $"{siteId2}",  "Equipment Lifecycle Risk", "", .5, "%"} );
            lines.Add(new List<object> { $"{portfolioId}", $"{siteId3}",  "Equipment Lifecycle Risk", "", .6, "%"} );
            lines.Add(new List<object> { $"{portfolioId}", $"{siteId4}",  "Equipment Lifecycle Risk", "", .3, "%"} );
            lines.Add(new List<object> { $"{portfolioId}", $"{siteId5}",  "Equipment Lifecycle Risk", "", .3, "%"} );
            lines.Add(new List<object> { $"{portfolioId}", $"{siteId6}",  "Equipment Lifecycle Risk", "", .2, "%"} );
            lines.Add(new List<object> { $"{portfolioId}", $"{siteId7}",  "Equipment Lifecycle Risk", "", .8, "%"} );
            lines.Add(new List<object> { $"{portfolioId}", $"{siteId8}",  "Equipment Lifecycle Risk", "", .7, "%"} );
            lines.Add(new List<object> { $"{portfolioId}", $"{siteId9}",  "Equipment Lifecycle Risk", "", .5, "%"} );
            lines.Add(new List<object> { $"{portfolioId}", $"{siteId10}", "Equipment Lifecycle Risk", "", .9, "%"} );

            IEnumerable<IEnumerable<object>> eLines = lines;

            return Task.FromResult(eLines);
        }

        private static Task<IEnumerable<IEnumerable<object>>> Buildings()
        {
            var lines = new List<List<object>>();
            var portfolioId = Guid.NewGuid().ToString();
            var siteId1  = Guid.Parse("404bd33c-a697-4027-b6a6-677e30a53d07");
            var siteId2  = Guid.Parse("a6b78f54-9875-47bc-9612-aa991cc464f3");
            var siteId3  = Guid.Parse("934638e3-4bd7-4749-bd52-bd6e47d0fbb2");
            var siteId4  = Guid.Parse("e719ac18-192b-4174-91db-b3a624f1f1a4");
            var siteId5  = Guid.Parse("092D18F5-F5CE-400F-B904-FD4F7BAF104B");
            var siteId6  = Guid.Parse("EC327557-950A-45CA-9B54-8D36752DBB6B");
            var siteId7  = Guid.Parse("180E1DCF-2F74-4D01-8019-E2D458CD7B87");
            var siteId8  = Guid.Parse("3A6D58E8-C8E0-4F30-AA58-38BAFB7937E2");
            var siteId9  = Guid.Parse("E9AE3306-D6FE-497B-BF82-1971C891525A");
            var siteId10 = Guid.Parse("C90B5874-8E9E-4AFB-994F-26E7E8EB8818");

            lines.Add(new List<object> { $"{portfolioId}", $"{siteId1}", "OperationsScore_LastValue", "", .98, "%"} );
            lines.Add(new List<object> { $"{portfolioId}", $"{siteId2}", "OperationsScore_LastValue", "", .92, "%"} );
            lines.Add(new List<object> { $"{portfolioId}", $"{siteId3}", "OperationsScore_LastValue", "", .88, "%"} );
            lines.Add(new List<object> { $"{portfolioId}", $"{siteId4}", "OperationsScore_LastValue", "", .75, "%"} );
            lines.Add(new List<object> { $"{portfolioId}", $"{siteId5}", "OperationsScore_LastValue", "", .72, "%"} );
            lines.Add(new List<object> { $"{portfolioId}", $"{siteId6}", "OperationsScore_LastValue", "", .67, "%"} );
            lines.Add(new List<object> { $"{portfolioId}", $"{siteId7}", "OperationsScore_LastValue", "", .53, "%"} );
            lines.Add(new List<object> { $"{portfolioId}", $"{siteId8}", "OperationsScore_LastValue", "", .52, "%"} );
            lines.Add(new List<object> { $"{portfolioId}", $"{siteId9}", "OperationsScore_LastValue", "", .48, "%"} );
            lines.Add(new List<object> { $"{portfolioId}", $"{siteId10}", "OperationsScore_LastValue", "", .32, "%"} );

            IEnumerable<IEnumerable<object>> eLines = lines;

            return Task.FromResult(eLines);
        }

        private static Task<IEnumerable<IEnumerable<object>>> DatedBuildingData()
        {
            var lines = new List<List<object>>();
            var portfolioId = Guid.NewGuid().ToString();

            lines.Add(new List<object> { $"{portfolioId}", $"{_twin1.SiteId}", "OperationsScore_LastValue", "2024/07/01", .33, "%" });
            lines.Add(new List<object> { $"{portfolioId}", $"{_twin1.SiteId}", "ComfortScore_LastValue", "2024/07/01", .33, "%" });
            lines.Add(new List<object> { $"{portfolioId}", $"{_twin1.SiteId}", "EnergyScore_LastValue", "2024/07/01", .33, "%" });
            lines.Add(new List<object> { $"{portfolioId}", $"{_twin1.SiteId}", "OperationsScore_LastValue", "2024/07/02", .44, "%" });
            lines.Add(new List<object> { $"{portfolioId}", $"{_twin1.SiteId}", "ComfortScore_LastValue", "2024/07/02", .44, "%" });
            lines.Add(new List<object> { $"{portfolioId}", $"{_twin1.SiteId}", "EnergyScore_LastValue", "2024/07/02", .44, "%" });
            lines.Add(new List<object> { $"{portfolioId}", $"{_twin1.SiteId}", "OperationsScore_LastValue", "2024/07/03", .55, "%" });
            lines.Add(new List<object> { $"{portfolioId}", $"{_twin1.SiteId}", "ComfortScore_LastValue", "2024/07/03", .55, "%" });
            lines.Add(new List<object> { $"{portfolioId}", $"{_twin1.SiteId}", "EnergyScore_LastValue", "2024/07/03", .55, "%" });
            lines.Add(new List<object> { $"{portfolioId}", $"{_twin1.SiteId}", "OperationsScore_LastValue", "2024/07/04", .66, "%" });
            lines.Add(new List<object> { $"{portfolioId}", $"{_twin1.SiteId}", "ComfortScore_LastValue", "2024/07/04", .66, "%" });
            lines.Add(new List<object> { $"{portfolioId}", $"{_twin1.SiteId}", "EnergyScore_LastValue", "2024/07/04", .66, "%" });
            lines.Add(new List<object> { $"{portfolioId}", $"{_twin1.SiteId}", "OperationsScore_LastValue", "2024/07/05", .77, "%" });
            lines.Add(new List<object> { $"{portfolioId}", $"{_twin1.SiteId}", "ComfortScore_LastValue", "2024/07/05", .77, "%" });
            lines.Add(new List<object> { $"{portfolioId}", $"{_twin1.SiteId}", "EnergyScore_LastValue", "2024/07/05", .77, "%" });
            lines.Add(new List<object> { $"{portfolioId}", $"{_twin1.SiteId}", "OperationsScore_LastValue", "2024/07/06", .88, "%" });
            lines.Add(new List<object> { $"{portfolioId}", $"{_twin1.SiteId}", "ComfortScore_LastValue", "2024/07/06", .88, "%" });
            lines.Add(new List<object> { $"{portfolioId}", $"{_twin1.SiteId}", "EnergyScore_LastValue", "2024/07/06", .88, "%" });
            lines.Add(new List<object> { $"{portfolioId}", $"{_twin1.SiteId}", "OperationsScore_LastValue", "2024/07/07", .98, "%" });
            lines.Add(new List<object> { $"{portfolioId}", $"{_twin1.SiteId}", "ComfortScore_LastValue", "2024/07/07", .98, "%" });
            lines.Add(new List<object> { $"{portfolioId}", $"{_twin1.SiteId}", "EnergyScore_LastValue", "2024/07/07", .98, "%" });

            IEnumerable<IEnumerable<object>> eLines = lines;

            return Task.FromResult(eLines);
        }
        

        private Task<IEnumerable<IEnumerable<object>>> OperationalTrends()
        {
            var lines = new List<List<object>>();
            var portfolioId = Guid.NewGuid().ToString();

            lines.Add(new List<object> { portfolioId, "", "Overall", "2021-08-01T00:00:00", .72} );
            lines.Add(new List<object> { portfolioId, "", "Overall", "2021-08-02T00:00:00", .74} );
            lines.Add(new List<object> { portfolioId, "", "Overall", "2021-08-03T00:00:00", .71} );
            lines.Add(new List<object> { portfolioId, "", "Overall", "2021-08-04T00:00:00", .69} );
            lines.Add(new List<object> { portfolioId, "", "Overall", "2021-08-05T00:00:00", .72} );
            lines.Add(new List<object> { portfolioId, "", "Overall", "2021-08-06T00:00:00", .71} );
            lines.Add(new List<object> { portfolioId, "", "Overall", "2021-08-07T00:00:00", .78} );
            lines.Add(new List<object> { portfolioId, "", "Overall", "2021-08-08T00:00:00", .74} );
            lines.Add(new List<object> { portfolioId, "", "Overall", "2021-08-09T00:00:00", .77} );
            lines.Add(new List<object> { portfolioId, "", "Overall", "2021-08-10T00:00:00", .78} );
                                                      
            lines.Add(new List<object> { portfolioId, "", "Energy Cost", "2021-08-01T00:00:00", .52} );
            lines.Add(new List<object> { portfolioId, "", "Energy Cost", "2021-08-02T00:00:00", .54} );
            lines.Add(new List<object> { portfolioId, "", "Energy Cost", "2021-08-03T00:00:00", .51} );
            lines.Add(new List<object> { portfolioId, "", "Energy Cost", "2021-08-04T00:00:00", .53} );
            lines.Add(new List<object> { portfolioId, "", "Energy Cost", "2021-08-05T00:00:00", .52} );
            lines.Add(new List<object> { portfolioId, "", "Energy Cost", "2021-08-06T00:00:00", .55} );
            lines.Add(new List<object> { portfolioId, "", "Energy Cost", "2021-08-07T00:00:00", .58} );
            lines.Add(new List<object> { portfolioId, "", "Energy Cost", "2021-08-08T00:00:00", .59} );
            lines.Add(new List<object> { portfolioId, "", "Energy Cost", "2021-08-09T00:00:00", .47} );
            lines.Add(new List<object> { portfolioId, "", "Energy Cost", "2021-08-10T00:00:00", .52} );
                                                       
            lines.Add(new List<object> { portfolioId, "", "Equipment Lifecycle Risk", "2021-08-01T00:00:00", .42} );
            lines.Add(new List<object> { portfolioId, "", "Equipment Lifecycle Risk", "2021-08-02T00:00:00", .44} );
            lines.Add(new List<object> { portfolioId, "", "Equipment Lifecycle Risk", "2021-08-03T00:00:00", .41} );
            lines.Add(new List<object> { portfolioId, "", "Equipment Lifecycle Risk", "2021-08-04T00:00:00", .43} );
            lines.Add(new List<object> { portfolioId, "", "Equipment Lifecycle Risk", "2021-08-05T00:00:00", .42} );
            lines.Add(new List<object> { portfolioId, "", "Equipment Lifecycle Risk", "2021-08-06T00:00:00", .45} );
            lines.Add(new List<object> { portfolioId, "", "Equipment Lifecycle Risk", "2021-08-07T00:00:00", .38} );
            lines.Add(new List<object> { portfolioId, "", "Equipment Lifecycle Risk", "2021-08-08T00:00:00", .39} );
            lines.Add(new List<object> { portfolioId, "", "Equipment Lifecycle Risk", "2021-08-09T00:00:00", .42} );
            lines.Add(new List<object> { portfolioId, "", "Equipment Lifecycle Risk", "2021-08-10T00:00:00", .42} );
                                                       
            lines.Add(new List<object> { portfolioId, "", "Comfort", "2021-08-01T00:00:00", .30} );
            lines.Add(new List<object> { portfolioId, "", "Comfort", "2021-08-02T00:00:00", .37} );
            lines.Add(new List<object> { portfolioId, "", "Comfort", "2021-08-03T00:00:00", .31} );
            lines.Add(new List<object> { portfolioId, "", "Comfort", "2021-08-04T00:00:00", .33} );
            lines.Add(new List<object> { portfolioId, "", "Comfort", "2021-08-05T00:00:00", .32} );
            lines.Add(new List<object> { portfolioId, "", "Comfort", "2021-08-06T00:00:00", .41} );
            lines.Add(new List<object> { portfolioId, "", "Comfort", "2021-08-07T00:00:00", .38} );
            lines.Add(new List<object> { portfolioId, "", "Comfort", "2021-08-08T00:00:00", .41} );
            lines.Add(new List<object> { portfolioId, "", "Comfort", "2021-08-09T00:00:00", .32} );
            lines.Add(new List<object> { portfolioId, "", "Comfort", "2021-08-10T00:00:00", .31} );

            IEnumerable<IEnumerable<object>> eLines = lines;

            return Task.FromResult(eLines);
        }

    }

}
