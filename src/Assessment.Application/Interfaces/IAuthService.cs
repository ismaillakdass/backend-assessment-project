using Assessment.Application.DTOs.Auth;

namespace Assessment.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto dto, CancellationToken cancellationToken = default);
    Task<bool> RegisterAsync(RegisterRequestDto dto, CancellationToken cancellationToken = default);
    Task LogoutAsync(string token, CancellationToken cancellationToken = default);
}
