using Assessment.Domain.Entities;
using Assessment.Domain.Enums;
using Assessment.Domain.Interfaces;
using Assessment.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Assessment.Infrastructure.Repositories;

public class DonationRepository : Repository<Donation>, IDonationRepository
{
    public DonationRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Donation>> GetByDonorIdAsync(int donorId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(d => d.DonorId == donorId)
            .OrderByDescending(d => d.DonationDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Donation>> GetPendingDonationsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(d => d.Donor)
            .Where(d => d.Status == DonationStatus.Pending)
            .OrderBy(d => d.DonationDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<Donation?> GetDonationWithDonorAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(d => d.Donor)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    /// <summary>
    /// Note: Candidates working on Task 4 can optimize bulk updates directly in EF Core 9
    /// using ExecuteUpdateAsync rather than loading records into memory.
    /// </summary>
    public async Task<int> BulkUpdateStatusAsync(
        DonationStatus currentStatus,
        DonationStatus newStatus,
        CancellationToken cancellationToken = default)
    {
        // Default inefficient approach loading into memory:
        var donations = await _dbSet.Where(d => d.Status == currentStatus).ToListAsync(cancellationToken);
        foreach (var donation in donations)
        {
            donation.Status = newStatus;
            donation.UpdatedAt = DateTime.UtcNow;
        }
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
