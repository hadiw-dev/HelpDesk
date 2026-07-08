using System.Net.Http.Json;
using System.Text.Json;
using HelpDesk.Application.Features.Auth.Dtos;
using HelpDesk.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace HelpDesk.IntegrationTests.Infrastructure;

/// <summary>
/// System.Net.Http.Json's default options are case-sensitive and don't apply a naming policy, but
/// the API serializes responses as camelCase — every request/response in these tests must use this
/// explicitly, or deserialized DTOs silently come back with default/empty property values.
/// </summary>
public static class TestJson
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);
}

/// <summary>
/// Shared setup helpers for integration tests that need a real, logged-in user. These tests run
/// against the real LocalDB (see <see cref="TestWebApplicationFactory"/>), so every registered user
/// gets a GUID-suffixed email to avoid unique-email collisions across repeated test runs.
/// </summary>
public static class AuthTestHelper
{
    public static async Task<(string Email, string Password, AuthResult Auth)> RegisterNewUserAsync(
        HttpClient client, string emailPrefix)
    {
        var email = $"{emailPrefix}.{Guid.NewGuid():N}@helpdesk.local";
        const string password = "Passw0rd!123";

        var response = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            Email = email,
            Password = password,
            ConfirmPassword = password,
            FirstName = "Integration",
            LastName = "Test",
        }, TestJson.Options);

        response.EnsureSuccessStatusCode();
        var auth = (await response.Content.ReadFromJsonAsync<AuthResult>(TestJson.Options))!;

        return (email, password, auth);
    }

    public static async Task<AuthResult> LoginAsync(HttpClient client, string email, string password)
    {
        var response = await client.PostAsJsonAsync(
            "/api/v1/auth/login", new { Email = email, Password = password }, TestJson.Options);
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<AuthResult>(TestJson.Options))!;
    }

    /// <summary>
    /// Roles are baked into the JWT at issuance, so a caller must log in again after promotion to
    /// get a token that actually carries the new role claim.
    /// </summary>
    public static async Task PromoteToRoleAsync(TestWebApplicationFactory factory, Guid userId, string role)
    {
        using var scope = factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var user = await userManager.FindByIdAsync(userId.ToString())
            ?? throw new InvalidOperationException($"Test setup error: user {userId} not found.");

        await userManager.AddToRoleAsync(user, role);
    }

    public static async Task<(string Email, string Password, AuthResult Auth)> RegisterAndPromoteAsync(
        HttpClient client, TestWebApplicationFactory factory, string emailPrefix, string role)
    {
        var (email, password, auth) = await RegisterNewUserAsync(client, emailPrefix);
        await PromoteToRoleAsync(factory, auth.User.Id, role);

        var refreshedAuth = await LoginAsync(client, email, password);
        return (email, password, refreshedAuth);
    }
}
