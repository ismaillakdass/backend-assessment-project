namespace Assessment.Application.DTOs.Donors;

public class DonorDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public decimal TotalDonatedAmount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateDonorDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}

public class UpdateDonorDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class DonorDetailDto : DonorDto
{
    public int DonationCount { get; set; }
    public DateTime? LastDonationDate { get; set; }
}

public class DonorReportSummaryDto
{
    public int DonorId { get; set; }
    public string DonorName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int TotalDonationsCount { get; set; }
    public decimal TotalDonatedAmount { get; set; }
    public decimal AverageDonationAmount { get; set; }
    public DateTime? LastDonationDate { get; set; }
    public string Status { get; set; } = string.Empty;
}
