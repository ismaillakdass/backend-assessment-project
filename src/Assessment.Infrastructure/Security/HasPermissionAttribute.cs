using Microsoft.AspNetCore.Authorization;

namespace Assessment.Infrastructure.Security;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = false)]
public class HasPermissionAttribute : AuthorizeAttribute
{
    public HasPermissionAttribute(string permission) : base(policy: permission)
    {
    }
}
