using HelpDesk.Application.Features.Auth.Dtos;

namespace HelpDesk.Application.Features.Auth.Interfaces;

public interface IAuthService
{
    Task<AuthResult> RegisterAsync(RegisterRequest request, string? ipAddress, CancellationToken cancellationToken = default);

    Task<AuthResult> LoginAsync(LoginRequest request, string? ipAddress, CancellationToken cancellationToken = default);

    Task<AuthResult> RefreshTokenAsync(string refreshToken, string? ipAddress, CancellationToken cancellationToken = default);

    Task LogoutAsync(string refreshToken, string? ipAddress, CancellationToken cancellationToken = default);

    Task ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default);

    Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default);

    Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken = default);

    Task<UserDto> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken cancellationToken = default);

    Task<UserDto> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default);
}
