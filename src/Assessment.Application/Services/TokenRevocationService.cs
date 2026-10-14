using Assessment.Application.Interfaces;
using Assessment.Domain.Entities;
using Assessment.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Assessment.Application.Services;

/// <summary>
/// TASK 11: JWT Logout / Token Revocation (5 Points)
/// This service is currently a dummy/incomplete skeleton.
/// Logout endpoint calls RevokeTokenAsync, but the token is never stored or checked,
/// meaning the JWT token remains valid and usable until its expiration date even after logout!
/// Candidates should implement a proper revocation/blacklist mechanism.
/// </summary>
public class TokenRevocationService : ITokenRevocationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TokenRevocationService> _logger;

    public TokenRevocationService(IUnitOfWork unitOfWork, ILogger<TokenRevocationService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task RevokeTokenAsync(string token, string? reason = null, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("Token revocation requested, but no revocation store is implemented!");

        // ANTI-PATTERN / FLAW [Task 11]:
        // Token is NOT stored in DB or cache, so logout is a no-op!
        // TODO [Task 11]: Implement storing the revoked token / JTI in the database using RevokedToken entity
        // or a distributed cache with expiration.
        await Task.CompletedTask;
    }

    public async Task<bool> IsTokenRevokedAsync(string token, CancellationToken cancellationToken = default)
    {
        // ANTI-PATTERN / FLAW [Task 11]:
        // Always returns false! Therefore revoked tokens are never rejected by auth middleware.
        // TODO [Task 11]: Check whether token exists in RevokedTokens table or cache.
        return await Task.FromResult(false);
    }
}
