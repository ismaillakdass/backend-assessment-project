namespace Assessment.Application.Interfaces;

public interface ITokenRevocationService
{
    Task RevokeTokenAsync(string token, string? reason = null, CancellationToken cancellationToken = default);
    Task<bool> IsTokenRevokedAsync(string token, CancellationToken cancellationToken = default);
}
