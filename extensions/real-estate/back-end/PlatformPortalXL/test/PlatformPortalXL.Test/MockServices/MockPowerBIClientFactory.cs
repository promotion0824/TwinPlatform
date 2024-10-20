using Microsoft.PowerBI.Api;
using Microsoft.PowerBI.Api.Models;
using Microsoft.Rest;
using Moq;
using PlatformPortalXL.Services.PowerBI;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PlatformPortalXL.Test.MockServices
{
    public class MockPowerBIClientFactory : IPowerBIClientFactory
    {
        public string EmbedReportToken { get; set; }
        public DateTime EmbedReportTokenExpiration { get; set; }
        public string EmbedReportUrl { get; set; }

        public IPowerBIClient Create(string token)
        {
            var reportsMock = new Mock<IReportsOperations>();
            reportsMock
                .Setup(x => x.GetReportInGroupWithHttpMessagesAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), null, It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new HttpOperationResponse<Report> { Body = new Report { EmbedUrl = this.EmbedReportUrl} }));
            reportsMock
                .Setup(x => x.GenerateTokenInGroupWithHttpMessagesAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<GenerateTokenRequest>(), null, It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new HttpOperationResponse<EmbedToken> { Body = new EmbedToken { Token = this.EmbedReportToken, Expiration = this.EmbedReportTokenExpiration } }));

            var clientMock = new Mock<IPowerBIClient>();
            clientMock
                .Setup(x => x.Reports)
                .Returns(reportsMock.Object);
            return clientMock.Object;
        }
    }
}
