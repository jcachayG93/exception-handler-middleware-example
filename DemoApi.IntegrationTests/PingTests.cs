using System.Net;
using DemoApi.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace DemoApi.IntegrationTests;

// https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-7.0

public class PingTests

{
    private readonly WebApplicationFactory<Program> _sutFactory;
    
    public PingTests()
    {
        _sutFactory = new WebApplicationFactory<Program>();
    }
    
    /// <summary>
    /// Use this method to get services from the DI Container
    /// </summary>
    private IServiceScope GetServiceFromDiContainer<T>(out T service)
    {
        var scope = this._sutFactory.Services.CreateScope();

        service = scope.ServiceProvider.GetService<T>()!;

        return scope;
    }


    


    [Fact]
    public async Task Ping_OnSuccess_ReturnsOkWithPongResponse()
    {
        // ************ ARRANGE ************
        
        var ep = "ping";

        var client = _sutFactory.CreateClient();

        // ************ ACT ****************

        var response = await client.GetAsync(ep);

        // ************ ASSERT *************
        
        Assert.True(response.IsSuccessStatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        
        Assert.Equal("Pong!", responseContent);
    }


    [Fact]
    public async Task Pjng_OnError_RetunsBadRequestWithExceptionMessage()
    {

        // ************ ARRANGE ************

        using var scope = GetServiceFromDiContainer(
            out PingService pingService);
        
        pingService.SetupToThrowException("Something went wrong!");

        var ep = "ping";

        var client = _sutFactory.CreateClient();

        // ************ ACT ****************

        var response = await client.GetAsync(ep);

        // ************ ASSERT *************
        
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        
        Assert.Equal("Something went wrong!", responseContent);
      
    }
}