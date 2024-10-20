using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Net;
using Willow.Common.Middlewares;

namespace Willow.Common.UnitTests;

public class ConfigureExceptionHandlerTests 
{

    [Fact]
    public async Task ThrowInternalServerError_ConfigureExceptionHandler_ReturnProblemDetails()
    {
        // Arrange
        var problemDetailsFactory = new Mock<ProblemDetailsFactory>();

        problemDetailsFactory.Setup(x => x.CreateProblemDetails(It.IsAny<HttpContext>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(new ProblemDetails()
            {
                Status = 500,
                Title = "Internal Server Error"
            });

        var hostBuilder = new WebHostBuilder()
            .Configure(app =>
            {
                app.ConfigureExceptionHandler();
                app.Run(context => throw new Exception("Test exception"));
            }).ConfigureServices(services =>
            {
                services.AddSingleton(problemDetailsFactory.Object);
            });
        var server = new TestServer(hostBuilder);
        var client = server.CreateClient();



        //Act
        var response = await client.GetAsync("/");
        var result  = await response.Content.ReadAsStringAsync();

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        result.Should().Contain("Internal Server Error");
        result.Should().NotContain("Test exception");

    }


    [Fact]
    public async Task RequestWithoutInternalServerError_ConfigureExceptionHandler_ShouldPassThroughWithNoException()
    {
        // Arrange

        var hostBuilder = new WebHostBuilder()
            .Configure(app =>
            {
                app.ConfigureExceptionHandler();
                app.Run(context => Task.CompletedTask);
            });
        var server = new TestServer(hostBuilder);
        var client = server.CreateClient();



        //Act
        var response = await client.GetAsync("/");
        var result = await response.Content.ReadAsStringAsync();

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);


    }
}