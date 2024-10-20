using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Autodesk.Forge.Model;
using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using PlatformPortalXL.Features.ContactUs;
using PlatformPortalXL.Models;
using Willow.Batch;
using Willow.Platform.Models;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.ContactUs;

public class CreateSupportTicketTests : BaseInMemoryTest
{
    public CreateSupportTicketTests(ITestOutputHelper output) : base(output)
    {
    }
    [Fact]
    public async Task requestValidWithoutInsightIds_CreateSupportTicket_Returns201()
    {
        var userId = Guid.NewGuid();
        var customer = Fixture.Create<Customer>();
        var site = Fixture.Build<Site>()
            .With(c => c.CustomerId, customer.Id)
            .Create();
        var request = Fixture.Build<CreateSupportTicketRequest>()
            .With(x => x.RequestorsEmail, "bob@bob.bob")
            .With(x => x.Url, "https://test-uat.com/")
            .With(c => c.SiteId, site.Id)
            .Without(c => c.InsightIds).Create();
       
        using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
        using (var client = server.CreateClientWithPermissionOnSite(userId, Permissions.ViewSites, site.Id))
        {
            server.Arrange().GetDirectoryApi()
                .SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId=view-sites")
                .ReturnsJson(new List<Site> { site });
            server.Arrange().GetDirectoryApi()
                .SetupRequest(HttpMethod.Get, $"customers/{site.CustomerId}")
                .ReturnsJson(customer);
          
            var response = await client.PostAsync("ContactUs", GetMultipartContent(request));

            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }
    }
    [Fact]
    public async Task requestValidWithInsightIds_CreateSupportTicket_Returns201()
    {
        var insightId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var customer = Fixture.Create<Customer>();
        var site = Fixture.Build<Site>()
            .With(c => c.CustomerId, customer.Id)
            .Create();
        var request = Fixture.Build<CreateSupportTicketRequest>()
            .With(x => x.RequestorsEmail, "bob@bob.bob")
            .With(x=>x.Url, "https://test-uat.com/")
            .With(c=>c.InsightIds,new List<Guid>(){ insightId })
            .With(c => c.SiteId, site.Id).Create();
        var expectedInsight = Fixture.Build<Insight>()
            .With(c => c.Id, insightId)
            .With(x => x.SiteId, site.Id)
            .Without(x => x.EquipmentId)
            .With(x => x.SourceType, InsightSourceType.App)
            .With(x => x.CustomerId, customer.Id)
            .Create();
        using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
        using (var client = server.CreateClientWithPermissionOnSite(userId, Permissions.ViewSites, site.Id))
        {
            server.Arrange().GetDirectoryApi()
                .SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId=view-sites")
                .ReturnsJson(new List<Site>{site});
            server.Arrange().GetDirectoryApi()
                .SetupRequest(HttpMethod.Get, $"customers/{site.CustomerId}")
                .ReturnsJson(customer);
            server.Arrange().GetInsightApi().SetupRequest(HttpMethod.Post, "insights?addFloor=False").ReturnsJson(new BatchDto<Insight>{ Items =new []{ expectedInsight }});
            server.Arrange().GetInsightApi().SetupRequest(HttpMethod.Put, $"sites/{site.Id}/insights/{expectedInsight.Id}").ReturnsJson(expectedInsight);
            var response = await client.PostAsync("ContactUs", GetMultipartContent(request));

            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }
    }
 
    [Fact]
    public async Task UserDoesNotHaveCorrectPermission_CreateSupportTicket_ReturnsForbidden()
    {
        var siteId= Guid.NewGuid();
        var request = Fixture.Build<CreateSupportTicketRequest>()
            .With(x=>x.RequestorsEmail, "bob@bob.bob")
            .With(x => x.Url, "https://test-uat.com/")
            .With(c => c.SiteId, siteId).Create();
        using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
        using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ViewSites, siteId))
        {
            server.Arrange().GetSiteApi()
                .SetupRequest(HttpMethod.Get, $"sites/{siteId}")
                .ReturnsJson(new Site{Id = siteId});
            var response = await client.PostAsync("ContactUs", GetMultipartContent(request));

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }
    }


    private MultipartFormDataContent GetMultipartContent(CreateSupportTicketRequest createTicketRequest)
    {
        var dataContent = new MultipartFormDataContent();
         
        dataContent.Add( new StringContent(createTicketRequest.Comment), "Comment");
        dataContent.Add(new StringContent(createTicketRequest.RequestorsEmail), "RequestorsEmail");
        dataContent.Add( new StringContent(createTicketRequest.RequestorsName), "RequestorsName");
        dataContent.Add( new StringContent(createTicketRequest.Subject ?? string.Empty), "Subject");
        dataContent.Add(new StringContent(createTicketRequest.Category.ToString()), "SkillCategory");
        dataContent.Add(new StringContent(createTicketRequest.SiteId.ToString()), "SiteId");
        dataContent.Add(new StringContent(createTicketRequest.Url.ToString()), "Url");
        if (createTicketRequest.InsightIds != null)
        {
            createTicketRequest.InsightIds.ForEach(c => dataContent.Add(new StringContent(c.ToString()), "InsightIds"));
        }
        return dataContent;
    }
}
