using Assessment.Domain.Entities;
using Assessment.Domain.Enums;

namespace Assessment.Domain.Interfaces;

public interface IDonationRepository : IRepository<Donation>
{
    Task<IReadOnlyList<Donation>> GetByDonorIdAsync(int donorId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Donation>> GetPendingDonationsAsync(CancellationToken cancellationToken = default);
    Task<Donation?> GetDonationWithDonorAsync(int id, CancellationToken cancellationToken = default);
    Task<int> BulkUpdateStatusAsync(DonationStatus currentStatus, DonationStatus newStatus, CancellationToken cancellationToken = default);
}
