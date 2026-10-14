using Assessment.Application.Common;
using Assessment.Application.DTOs.Donations;
using Assessment.Domain.Enums;

namespace Assessment.Application.Interfaces;

public interface IDonationService
{
    Task<DonationDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DonationDto>> GetByDonorIdAsync(int donorId, CancellationToken cancellationToken = default);
    Task<PagedResult<DonationDto>> GetPagedAsync(DonationFilterDto filter, PaginationFilter pagination, CancellationToken cancellationToken = default);
    Task<DonationDto> CreateDonationAsync(CreateDonationDto dto, string? currentUserId = null, CancellationToken cancellationToken = default);
    Task<DonationDto> ApproveDonationAsync(int id, DonationApprovalDto dto, string approvedByUserId, CancellationToken cancellationToken = default);
    Task<int> BulkUpdateStatusAsync(BulkUpdateDonationStatusDto dto, CancellationToken cancellationToken = default);
    Task<int> CancelPendingDonationsOlderThanDaysAsync(int days, CancellationToken cancellationToken = default);
}
