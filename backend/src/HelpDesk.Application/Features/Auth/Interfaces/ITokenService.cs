using HelpDesk.Domain.Identity;

namespace HelpDesk.Application.Features.Auth.Interfaces;

public interface ITokenService
{
    (string AccessToken, DateTime ExpiresAt) GenerateAccessToken(ApplicationUser user, IEnumerable<string> roles);

    string GenerateRefreshToken();
}
