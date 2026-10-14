using Assessment.Domain.Common;

namespace Assessment.Domain.Entities;

public class AuditLog : BaseEntity
{
    public string EntityName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Details { get; set; } = string.Empty;
    public string? UserId { get; set; }
}
