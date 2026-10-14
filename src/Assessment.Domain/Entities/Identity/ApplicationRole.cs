using Microsoft.AspNetCore.Identity;

namespace Assessment.Domain.Entities.Identity;

public class ApplicationRole : IdentityRole
{
    public string? Description { get; set; }
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
