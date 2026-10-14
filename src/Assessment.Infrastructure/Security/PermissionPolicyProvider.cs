using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Assessment.Infrastructure.Security;

/// <summary>
/// Dynamically creates policies for any permission name requested in [HasPermission("permission.name")]
/// or [Authorize(Policy = "permission.name")].
/// </summary>
public class PermissionPolicyProvider : DefaultAuthorizationPolicyProvider
{
    private readonly AuthorizationOptions _options;

    public PermissionPolicyProvider(IOptions<AuthorizationOptions> options) : base(options)
    {
        _options = options.Value;
    }

    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        var policy = await base.GetPolicyAsync(policyName);
        if (policy != null)
        {
            return policy;
        }

        // Dynamically create policy containing PermissionRequirement for this permission name
        var permissionPolicy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .AddRequirements(new PermissionRequirement(policyName))
            .Build();

        return permissionPolicy;
    }
}
