using Assessment.Domain.Enums;

namespace Assessment.Application.DTOs.Donations;

public class DonationDto
{
    public int Id { get; set; }
    public int DonorId { get; set; }
    public string DonorName { get; set; } = string.Empty;
    public string DonorEmail { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DonationType DonationType { get; set; }
    public string DonationTypeDescription => DonationType.ToString();
    public DateTime DonationDate { get; set; }
    public DonationStatus Status { get; set; }
    public string StatusDescription => Status.ToString();
    public string? Notes { get; set; }
    public string? ApprovedByUserId { get; set; }
    public DateTime? ApprovedAt { get; set; }
}

public class CreateDonationDto
{
    public int DonorId { get; set; }
    public decimal Amount { get; set; }
    public DonationType DonationType { get; set; } = DonationType.OneTime;
    public DateTime DonationDate { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }
}

public class BulkUpdateDonationStatusDto
{
    public DonationStatus CurrentStatus { get; set; } = DonationStatus.Pending;
    public DonationStatus NewStatus { get; set; } = DonationStatus.Approved;
}

public class DonationApprovalDto
{
    public string? Notes { get; set; }
}

public class DonationFilterDto
{
    public int? DonorId { get; set; }
    public DonationStatus? Status { get; set; }
    public DonationType? DonationType { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
}
