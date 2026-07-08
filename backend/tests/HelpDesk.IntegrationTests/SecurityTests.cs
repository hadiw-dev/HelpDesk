using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using HelpDesk.Application.Common.Models;
using HelpDesk.Application.Features.Lookups.Dtos;
using HelpDesk.Application.Features.Tickets.Dtos;
using HelpDesk.IntegrationTests.Infrastructure;

namespace HelpDesk.IntegrationTests;

/// <summary>
/// Exercises the specific security properties Phase 7 asked to "verify": response security headers,
/// CORS origin allowlisting, and that SQL-injection/XSS-style input is handled safely (parameterized
/// queries and JSON encoding, respectively) rather than causing a server error or getting executed.
/// </summary>
[Collection(ApiCollection.Name)]
public class SecurityTests
{
    private readonly HttpClient _client;

    public SecurityTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task AnyResponse_IncludesBaselineSecurityHeaders()
    {
        var response = await _client.GetAsync("/api/v1/ping");

        Assert.True(response.Headers.TryGetValues("X-Content-Type-Options", out var nosniff));
        Assert.Equal("nosniff", nosniff!.Single());

        Assert.True(response.Headers.TryGetValues("X-Frame-Options", out var frameOptions));
        Assert.Equal("DENY", frameOptions!.Single());

        Assert.True(response.Headers.TryGetValues("Referrer-Policy", out var referrerPolicy));
        Assert.Equal("no-referrer", referrerPolicy!.Single());
    }

    [Fact]
    public async Task Cors_AllowedOrigin_IsEchoedBackInResponseHeader()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/ping");
        request.Headers.Add("Origin", "http://localhost:5173");

        var response = await _client.SendAsync(request);

        Assert.True(response.Headers.TryGetValues("Access-Control-Allow-Origin", out var allowOrigin));
        Assert.Equal("http://localhost:5173", allowOrigin!.Single());
    }

    [Fact]
    public async Task Cors_DisallowedOrigin_GetsNoAccessControlHeader()
    {
        // The CORS middleware doesn't block the request server-side — it simply omits the header,
        // which is what makes the browser's own same-origin policy reject the response client-side.
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/ping");
        request.Headers.Add("Origin", "http://evil.example.com");

        var response = await _client.SendAsync(request);

        Assert.False(response.Headers.TryGetValues("Access-Control-Allow-Origin", out _));
    }

    [Theory]
    [InlineData("' OR '1'='1")]
    [InlineData("'; DROP TABLE Tickets; --")]
    [InlineData("x'); DELETE FROM Tickets WHERE ('a'='a")]
    public async Task TicketSearch_WithSqlInjectionStylePayload_ReturnsOkNotServerError(string payload)
    {
        var (_, _, auth) = await AuthTestHelper.RegisterNewUserAsync(_client, "sqli-search");

        using var request = new HttpRequestMessage(
            HttpMethod.Get, $"/api/v1/tickets?searchTerm={Uri.EscapeDataString(payload)}&page=1&pageSize=10");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PagedResult<TicketListItemDto>>(TestJson.Options);
        Assert.NotNull(result);
    }

    [Fact]
    public async Task CreateTicket_WithScriptTagInTitle_IsStoredAndReturnedAsPlainJsonText()
    {
        // This is a JSON API, not server-rendered HTML — the correct defense is that the payload
        // comes back as an inert JSON string (never executed here), not that it gets stripped.
        // A browser-based frontend is responsible for encoding it on render, which is covered
        // separately by React's default JSX escaping.
        var (_, _, auth) = await AuthTestHelper.RegisterNewUserAsync(_client, "xss-create");
        const string maliciousTitle = "<script>alert('xss')</script>";

        using var categoriesRequest = new HttpRequestMessage(HttpMethod.Get, "/api/v1/lookups/categories");
        categoriesRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var categoriesResponse = await _client.SendAsync(categoriesRequest);
        categoriesResponse.EnsureSuccessStatusCode();
        var categories = await categoriesResponse.Content.ReadFromJsonAsync<List<LookupItemDto>>(TestJson.Options);

        using var prioritiesRequest = new HttpRequestMessage(HttpMethod.Get, "/api/v1/lookups/priorities");
        prioritiesRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var prioritiesResponse = await _client.SendAsync(prioritiesRequest);
        prioritiesResponse.EnsureSuccessStatusCode();
        var priorities = await prioritiesResponse.Content.ReadFromJsonAsync<List<LookupItemDto>>(TestJson.Options);

        using var createRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/tickets")
        {
            Content = JsonContent.Create(new CreateTicketRequest
            {
                Title = maliciousTitle,
                Description = "Testing stored-XSS-style input handling.",
                CategoryId = categories![0].Id,
                PriorityId = priorities![0].Id,
            }, options: TestJson.Options),
        };
        createRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var createResponse = await _client.SendAsync(createRequest);

        createResponse.EnsureSuccessStatusCode();
        Assert.Equal("application/json; charset=utf-8", createResponse.Content.Headers.ContentType?.ToString());

        var created = await createResponse.Content.ReadFromJsonAsync<TicketDto>(TestJson.Options);
        Assert.Equal(maliciousTitle, created!.Title);
    }
}
