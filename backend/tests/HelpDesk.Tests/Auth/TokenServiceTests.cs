using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using HelpDesk.Application.Common.Options;
using HelpDesk.Domain.Identity;
using HelpDesk.Infrastructure.Services;
using Microsoft.Extensions.Options;

namespace HelpDesk.Tests.Auth;

public class TokenServiceTests
{
    private static TokenService CreateSut() => new(Options.Create(new JwtOptions
    {
        Issuer = "HelpDeskSystem.Tests",
        Audience = "HelpDeskSystem.Tests.Client",
        SecretKey = "unit-test-signing-key-that-is-long-enough-1234567890",
        ExpiryMinutes = 15,
        RefreshTokenExpiryDays = 7,
    }));

    [Fact]
    public void GenerateAccessToken_IncludesExpectedClaims()
    {
        var sut = CreateSut();
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "agent@helpdesk.local",
            FirstName = "Ivy",
            LastName = "Support",
        };

        var (accessToken, expiresAt) = sut.GenerateAccessToken(user, ["IT Support Agent"]);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);

        Assert.Equal(user.Id.ToString(), jwt.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
        Assert.Equal(user.Email, jwt.Claims.First(c => c.Type == ClaimTypes.Email).Value);
        Assert.Contains(jwt.Claims, c => c.Type == ClaimTypes.Role && c.Value == "IT Support Agent");
        Assert.True(expiresAt > DateTime.UtcNow);
    }

    [Fact]
    public void GenerateRefreshToken_ProducesUniqueValues()
    {
        var sut = CreateSut();

        var first = sut.GenerateRefreshToken();
        var second = sut.GenerateRefreshToken();

        Assert.NotEmpty(first);
        Assert.NotEqual(first, second);
    }
}
