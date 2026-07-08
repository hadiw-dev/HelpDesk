using System.Net;
using HelpDesk.IntegrationTests.Infrastructure;

namespace HelpDesk.IntegrationTests;

[Collection(ApiCollection.Name)]
public class ApiSmokeTests
{
    private readonly HttpClient _client;

    public ApiSmokeTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task HealthEndpoint_ReturnsOk()
    {
        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task PingEndpoint_ReturnsOk_ForVersionedRoute()
    {
        var response = await _client.GetAsync("/api/v1/ping");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Swagger_JsonDocument_IsAvailable()
    {
        var response = await _client.GetAsync("/swagger/v1/swagger.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
