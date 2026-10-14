using Assessment.Domain.Common;
using Assessment.Domain.Enums;

namespace Assessment.Domain.Entities;

public class Donation : BaseEntity
{
    public int DonorId { get; set; }
    public Donor? Donor { get; set; }

    public decimal Amount { get; set; }
    public DonationType DonationType { get; set; } = DonationType.OneTime;
    public DateTime DonationDate { get; set; } = DateTime.UtcNow;
    public DonationStatus Status { get; set; } = DonationStatus.Pending;

    public string? Notes { get; set; }
    public string? ApprovedByUserId { get; set; }
    public DateTime? ApprovedAt { get; set; }
}
