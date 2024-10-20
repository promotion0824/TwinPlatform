namespace Willow.AzureDigitalTwins.Services.UnitTests.Fixtures
{
    using Moq;
    using Willow.AzureDigitalTwins.Services.Domain.InMemory.Readers;
    using Willow.AzureDigitalTwins.Services.Domain.InMemory.Writers;
    using Willow.AzureDigitalTwins.Services.Interfaces;

    public class AzureDigitalTwinSyncServiceFixture : AzureDigitalTwinServiceFixture
    {
        public Mock<IAzureDigitalTwinWriter> AzureDigitalTwinWriterMock { get; set; }

        public Mock<IAzureDigitalTwinReader> AzureDigitalTwinReaderMock { get; set; }

        protected override IAzureDigitalTwinWriter GetServiceWriterInstance()
        {
            AzureDigitalTwinWriterMock = new Mock<IAzureDigitalTwinWriter>();

            return new InMemoryInstanceTwinWriter(AzureDigitalTwinModelParserMock.Object, AzureDigitalTwinCacheProviderMock.Object, AzureDigitalTwinWriterMock.Object);
        }

        protected override IAzureDigitalTwinReader GetServiceReaderInstance()
        {
            AzureDigitalTwinReaderMock = new Mock<IAzureDigitalTwinReader>();

            return new InMemoryInstanceTwinReader(AzureDigitalTwinReaderMock.Object, AzureDigitalTwinModelParserMock.Object, AzureDigitalTwinCacheProviderMock.Object);
        }
    }
}
