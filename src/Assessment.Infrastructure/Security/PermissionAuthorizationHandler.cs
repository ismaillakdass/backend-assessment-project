using System.Security.Claims;
using Assessment.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Assessment.Infrastructure.Security;

/// <summary>
/// TASK 3: Authorization / Permission System (15 Points)
/// This handler is supposed to evaluate whether the authenticated user has the required permission
/// either via JWT claims or directly from database-backed RolePermissions.
/// Currently, it only checks role claim == "Admin", completely ignoring the requirement and database permissions,
/// causing non-admin users (such as Operator) who have 'donations.approve' to be denied,
/// or failing to protect endpoints properly!
/// </summary>
public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PermissionAuthorizationHandler> _logger;

    public PermissionAuthorizationHandler(
        IServiceScopeFactory scopeFactory,
        ILogger<PermissionAuthorizationHandler> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (context.User?.Identity?.IsAuthenticated != true)
        {
            return;
        }

        // FLAW / INCOMPLETE IMPLEMENTATION [Task 3]:
        // Currently hardcoded to only succeed for Admin role!
        if (context.User.IsInRole("Admin"))
        {
            context.Succeed(requirement);
            return;
        }

        // 1. Check if token contains the permission claim:
        var hasClaim = context.User.Claims.Any(c =>
            c.Type == "Permission" &&
            string.Equals(c.Value, requirement.Permission, StringComparison.OrdinalIgnoreCase));

        if (hasClaim)
        {
            context.Succeed(requirement);
            return;
        }

        // TODO [Task 3]:
        // If the claim is not directly in the JWT token (e.g. roles/permissions updated since token issue),
        // query the database (ApplicationDbContext) to check if any of the user's roles has this permission assigned.
        // Currently this DB check is missing!
        
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return;
        }

        // Candidates should implement the query linking user -> roles -> RolePermissions -> Permission
        // and call context.Succeed(requirement) if permitted.

        _logger.LogWarning("User {UserId} lacks required permission: {Permission}", userId, requirement.Permission);
        await Task.CompletedTask;
    }
}
