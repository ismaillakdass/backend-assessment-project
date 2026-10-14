using Assessment.Application.DTOs.Donors;
using Assessment.Application.Interfaces;
using Assessment.Domain.Enums;
using Assessment.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Assessment.Application.Services;

/// <summary>
/// TASK 2: EF Core Performance Problem (20 Points)
/// This service contains severe EF Core performance anti-patterns:
/// - Excessive eager loading without AsNoTracking
/// - Early in-memory materialization (early ToListAsync)
/// - In-memory filtering instead of database-level WHERE clauses
/// - N+1 database queries inside a loop
/// - Lack of SQL projections (Select) causing huge memory overhead
/// </summary>
public class DonorReportService : IDonorReportService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DonorReportService> _logger;

    public DonorReportService(IUnitOfWork unitOfWork, ILogger<DonorReportService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<IReadOnlyList<DonorReportSummaryDto>> GetDonorSummaryReportAsync(
        decimal? minTotalAmount = null,
        int? minDonationsCount = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating donor summary report...");

        // CRITICAL BUG / ANTI-PATTERN [Task 2]:
        // 1. Pulls all Donors and all related Donations into EF tracking memory!
        // 2. Early ToListAsync loads full entity graphs into memory.
        var allDonors = await _unitOfWork.Donors.Query()
            .Include(d => d.Donations)
            .ToListAsync(cancellationToken);

        var reportList = new List<DonorReportSummaryDto>();

        // 3. Loops through each donor in-memory
        foreach (var donor in allDonors)
        {
            // 4. In-memory filtering instead of pushing query to SQLite / DB
            var approvedDonations = donor.Donations
                .Where(d => d.Status == DonationStatus.Approved)
                .ToList();

            var totalAmount = approvedDonations.Sum(d => d.Amount);
            var count = approvedDonations.Count;

            // In-memory filter criteria check
            if (minTotalAmount.HasValue && totalAmount < minTotalAmount.Value)
            {
                continue;
            }

            if (minDonationsCount.HasValue && count < minDonationsCount.Value)
            {
                continue;
            }

            // 5. N+1 Query: Querying DB for each donor inside the loop to get last donation date!
            var lastDonation = await _unitOfWork.Donations.Query()
                .Where(d => d.DonorId == donor.Id)
                .OrderByDescending(d => d.DonationDate)
                .FirstOrDefaultAsync(cancellationToken);

            var avgAmount = count > 0 ? totalAmount / count : 0m;

            reportList.Add(new DonorReportSummaryDto
            {
                DonorId = donor.Id,
                DonorName = $"{donor.FirstName} {donor.LastName}".Trim(),
                Email = donor.Email,
                TotalDonationsCount = count,
                TotalDonatedAmount = totalAmount,
                AverageDonationAmount = Math.Round(avgAmount, 2),
                LastDonationDate = lastDonation?.DonationDate,
                Status = donor.IsActive ? "Active" : "Inactive"
            });
        }

        return reportList;
    }
}
