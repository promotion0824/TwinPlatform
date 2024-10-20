namespace Willow.Msm.Connector.Tests
{
    using Microsoft.Extensions.Options;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Willow.Msm.Connector.Models;

    /// <summary>
    /// Tests for the Willow Twin Client.
    /// </summary>
    public partial class WillowClientTests
    {
        private CarbonActivityRequestMessage? carbonActivityRequestMessage;
        private IOptions<MsmFunctionOptions>? msmFunctionOptions;
        private readonly IHttpClientFactory httpClientFactory;

        public WillowClientTests(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
        }

        [TestInitialize]
        public void TestInitialize(IHttpClientFactory httpClientFactory)
        {
            this.carbonActivityRequestMessage = new CarbonActivityRequestMessage()
            {
                EnergyType = "Electricity",
                AggregationWindow = "None",
                OrganizationShortName = "ddk",
                OrganizationalUnitId = "DDK Investments",
                ClientId = "af6d2ea2e9364b26ae6d565b5b77847c",
                ClientSecret = "4ns8hwRqCAV4vFrPx10mXcSz4CjTeTiV",
                WatermarkDate = DateTime.Parse("2023-02-01"),
            };

            this.msmFunctionOptions = Options.Create(new MsmFunctionOptions()
            {
                RequiredModelIds = new List<string> { "dtmi:com:willowinc:Portfolio;1", "dtmi:com:willowinc:Building;1", "dtmi:com:willowinc:UtilityAccount;1", "dtmi:com:willowinc:BilledActiveElectricalEnergy;1", "dtmi:com:willowinc:BilledNaturalGasEnergy;1" },
            });
        }
    }
}
