using Assessment.Domain.Common;

namespace Assessment.Domain.Entities;

public class RevokedToken : BaseEntity
{
    public string Token { get; set; } = string.Empty;
    public DateTime RevokedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public string? Reason { get; set; }
}
