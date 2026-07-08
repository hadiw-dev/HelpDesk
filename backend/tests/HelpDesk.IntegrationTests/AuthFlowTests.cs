using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using HelpDesk.Application.Features.Auth.Dtos;
using HelpDesk.IntegrationTests.Infrastructure;

namespace HelpDesk.IntegrationTests;

[Collection(ApiCollection.Name)]
public class AuthFlowTests
{
    private readonly HttpClient _client;

    public AuthFlowTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_WithNewEmail_ReturnsTokensAndDefaultRole()
    {
        var (_, _, auth) = await AuthTestHelper.RegisterNewUserAsync(_client, "register-new");

        Assert.NotEmpty(auth.AccessToken);
        Assert.NotEmpty(auth.RefreshToken);
        Assert.Contains("Employee", auth.User.Roles);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsConflict()
    {
        var (email, _, _) = await AuthTestHelper.RegisterNewUserAsync(_client, "register-dup");

        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            Email = email,
            Password = "Passw0rd!123",
            ConfirmPassword = "Passw0rd!123",
            FirstName = "Dup",
            LastName = "User",
        }, TestJson.Options);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithCorrectCredentials_ReturnsTokens()
    {
        var (email, password, _) = await AuthTestHelper.RegisterNewUserAsync(_client, "login-ok");

        var auth = await AuthTestHelper.LoginAsync(_client, email, password);

        Assert.NotEmpty(auth.AccessToken);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        var (email, _, _) = await AuthTestHelper.RegisterNewUserAsync(_client, "login-bad");

        var response = await _client.PostAsJsonAsync(
            "/api/v1/auth/login", new { Email = email, Password = "WrongPassword!" }, TestJson.Options);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithUnknownEmail_ReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new { Email = $"nobody.{Guid.NewGuid():N}@helpdesk.local", Password = "whatever" },
            TestJson.Options);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RefreshToken_WithValidToken_IssuesNewAccessToken()
    {
        var (_, _, auth) = await AuthTestHelper.RegisterNewUserAsync(_client, "refresh-ok");

        var response = await _client.PostAsJsonAsync(
            "/api/v1/auth/refresh-token", new { RefreshToken = auth.RefreshToken }, TestJson.Options);
        response.EnsureSuccessStatusCode();

        var refreshed = await response.Content.ReadFromJsonAsync<AuthResult>(TestJson.Options);
        Assert.NotNull(refreshed);
        Assert.NotEmpty(refreshed!.AccessToken);
    }

    [Fact]
    public async Task RefreshToken_WithInvalidToken_ReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/auth/refresh-token", new { RefreshToken = "not-a-real-token" }, TestJson.Options);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetProfile_WithoutToken_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/v1/auth/profile");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetProfile_WithValidToken_ReturnsOwnProfile()
    {
        var (email, _, auth) = await AuthTestHelper.RegisterNewUserAsync(_client, "profile-ok");

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/auth/profile");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var response = await _client.SendAsync(request);

        response.EnsureSuccessStatusCode();
        var profile = await response.Content.ReadFromJsonAsync<UserDto>(TestJson.Options);
        Assert.Equal(email, profile!.Email);
    }

    [Fact]
    public async Task GetProfile_WithTamperedToken_ReturnsUnauthorized()
    {
        var (_, _, auth) = await AuthTestHelper.RegisterNewUserAsync(_client, "tampered");

        // Flip the last character of the signature segment — the payload still looks well-formed,
        // but the signature must no longer verify.
        var tampered = auth.AccessToken[..^1] + (auth.AccessToken[^1] == 'A' ? 'B' : 'A');

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/auth/profile");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tampered);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
