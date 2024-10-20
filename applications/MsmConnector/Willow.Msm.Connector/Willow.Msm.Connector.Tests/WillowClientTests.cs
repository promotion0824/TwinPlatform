namespace Willow.Msm.Connector.Tests
{
    using Microsoft.Extensions.Logging;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Willow.Msm.Connector.Services;

    /// <summary>
    /// Tests for the Willow Twin Client.
    /// </summary>
    [TestClass]
    public partial class WillowClientTests
    {
        [TestMethod]
        [TestCategory("Integration")]
        public async Task GetToken_ReturnsTokenWithValidExpiryDate()
        {
            var log = new Mock<ILogger>();

            var willowClient = new WillowClient(this.msmFunctionOptions!, this.httpClientFactory);
            willowClient.Initialise(this.carbonActivityRequestMessage!, log.Object);

            var token = await willowClient.GetToken();

            Assert.IsTrue(token.ExpiryDate > DateTime.UtcNow.AddMinutes(55));
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task GetAllFacilities_ReturnsAtLeastOneFacility()
        {
            var log = new Mock<ILogger>();
            var willowClient = new WillowClient(this.msmFunctionOptions!, this.httpClientFactory);
            willowClient.Initialise(this.carbonActivityRequestMessage!, log.Object);
            await willowClient.GetToken();

            var facilities = await willowClient.GetAllFacilities();

            Assert.IsTrue(facilities.Count > 0);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task GetPurchasedEnergyTwinDetailsForKnownSiteId_ReturnsAtLeastOneSetOfTwinDetails()
        {
            var wellKnownSiteId = "a226929d-6e27-480f-b8dd-40ffbc47024c";
            var log = new Mock<ILogger>();
            var willowClient = new WillowClient(this.msmFunctionOptions!, this.httpClientFactory);
            willowClient.Initialise(this.carbonActivityRequestMessage!, log.Object);
            await willowClient.GetToken();
            await willowClient.GetAllFacilities();

            var twinDetails = await willowClient.GetMsmRelatedTwinDetailsForFacility(wellKnownSiteId);

            Assert.IsTrue(twinDetails.Count > 0);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task GetPurchasedEnergyQuantitiesForKnownSiteId_ReturnsAtLeastOneQuantity()
        {
            var wellKnownSiteId = "a226929d-6e27-480f-b8dd-40ffbc47024c";
            var log = new Mock<ILogger>();
            var willowClient = new WillowClient(this.msmFunctionOptions!, this.httpClientFactory);
            willowClient.Initialise(this.carbonActivityRequestMessage!, log.Object);
            await willowClient.GetToken();
            await willowClient.GetAllFacilities();

            await willowClient.GetMsmRelatedTwinDetailsForFacility(wellKnownSiteId);
            var twinDetails = await willowClient.GetPurchasedEnergyQuantityForFacility(wellKnownSiteId);

            Assert.IsTrue(twinDetails[0].Quantities!.Count > 0);
        }
    }
}
