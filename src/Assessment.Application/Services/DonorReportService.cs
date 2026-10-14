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

        var query = _unitOfWork.Donors.Query().AsNoTracking();

        var report = query.Select(d => new DonorReportSummaryDto
        {
            DonorId = d.Id,
            DonorName = (d.FirstName + " " + d.LastName).Trim(),
            Email = d.Email,
            TotalDonationsCount = d.Donations.Count(x => x.Status == DonationStatus.Approved),
            TotalDonatedAmount = d.Donations
                .Where(x => x.Status == DonationStatus.Approved)
                .Sum(x => (decimal?)x.Amount) ?? 0m,
            LastDonationDate = d.Donations.Max(x => (DateTime?)x.DonationDate),
            Status = d.IsActive ? "Active" : "Inactive"
        });

        // 3) Filtreler: dolu geldiyse uygulanır
        if (minTotalAmount.HasValue)
        {
            report = report.Where(r => r.TotalDonatedAmount >= minTotalAmount.Value);
        }

        if (minDonationsCount.HasValue)
        {
            report = report.Where(r => r.TotalDonationsCount >= minDonationsCount.Value);
        }

        // 4) Tek sorgu ile çek
        var rows = await report
            .OrderBy(r => r.DonorId)
            .ToListAsync(cancellationToken);

        // 5) Ortalama bellekte, sadece sonuç satırları üzerinde
        foreach (var row in rows)
        {
            row.AverageDonationAmount = row.TotalDonationsCount > 0
                ? Math.Round(row.TotalDonatedAmount / row.TotalDonationsCount, 2)
                : 0m;
        }

        return rows;
    }
}
