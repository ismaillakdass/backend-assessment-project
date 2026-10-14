namespace Assessment.Domain.Entities.Identity;

public class RolePermission
{
    public string RoleId { get; set; } = string.Empty;
    public ApplicationRole? Role { get; set; }

    public int PermissionId { get; set; }
    public Permission? Permission { get; set; }
}
