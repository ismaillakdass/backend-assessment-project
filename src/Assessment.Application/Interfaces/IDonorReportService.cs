using Assessment.Application.DTOs.Donors;

namespace Assessment.Application.Interfaces;

public interface IDonorReportService
{
    Task<IReadOnlyList<DonorReportSummaryDto>> GetDonorSummaryReportAsync(decimal? minTotalAmount = null, int? minDonationsCount = null, CancellationToken cancellationToken = default);
}
