using System.Net;
using System.Net.Http.Headers;
using HelpDesk.Domain.Identity;
using HelpDesk.IntegrationTests.Infrastructure;

namespace HelpDesk.IntegrationTests;

/// <summary>
/// Verifies role/policy-gated endpoints reject the right way: 401 with no token at all, 403 with a
/// valid token that lacks the required role. Uses the demo endpoints on PingController
/// (<c>/admin</c> = Roles=Admin, <c>/management</c> = RequireManagerOrAdmin policy) as fixed,
/// unambiguous probes for this, since they don't depend on any ticket/business data existing.
/// </summary>
[Collection(ApiCollection.Name)]
public class AuthorizationTests
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory _factory;

    public AuthorizationTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private static HttpRequestMessage AuthedGet(string url, string accessToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return request;
    }

    [Fact]
    public async Task AdminEndpoint_WithoutToken_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/v1/ping/admin");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AdminEndpoint_WithEmployeeToken_ReturnsForbidden()
    {
        var (_, _, auth) = await AuthTestHelper.RegisterNewUserAsync(_client, "authz-employee");

        var response = await _client.SendAsync(AuthedGet("/api/v1/ping/admin", auth.AccessToken));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AdminEndpoint_WithAdminToken_ReturnsOk()
    {
        var (_, _, auth) = await AuthTestHelper.RegisterAndPromoteAsync(
            _client, _factory, "authz-admin", RoleNames.Admin);

        var response = await _client.SendAsync(AuthedGet("/api/v1/ping/admin", auth.AccessToken));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ManagementEndpoint_WithEmployeeToken_ReturnsForbidden()
    {
        var (_, _, auth) = await AuthTestHelper.RegisterNewUserAsync(_client, "authz-mgmt-employee");

        var response = await _client.SendAsync(AuthedGet("/api/v1/ping/management", auth.AccessToken));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task ManagementEndpoint_WithManagerToken_ReturnsOk()
    {
        var (_, _, auth) = await AuthTestHelper.RegisterAndPromoteAsync(
            _client, _factory, "authz-manager", RoleNames.Manager);

        var response = await _client.SendAsync(AuthedGet("/api/v1/ping/management", auth.AccessToken));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task DeleteTicket_WithEmployeeToken_ReturnsForbidden()
    {
        // RequireAgentOrAbove-gated; an Employee token should never reach the handler regardless of
        // whether the ticket id exists — the policy check runs before the not-found lookup would.
        var (_, _, auth) = await AuthTestHelper.RegisterNewUserAsync(_client, "authz-delete-employee");

        using var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/v1/tickets/{Guid.NewGuid()}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AdminUsersEndpoint_WithoutToken_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/v1/admin/users");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
