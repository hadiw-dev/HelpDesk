namespace HelpDesk.Application.Features.Auth.Dtos;

public class AuthResult
{
    public string AccessToken { get; set; } = string.Empty;
    public DateTime AccessTokenExpiresAt { get; set; }
    public string RefreshToken { get; set; } = string.Empty;
    public UserDto User { get; set; } = null!;
}
