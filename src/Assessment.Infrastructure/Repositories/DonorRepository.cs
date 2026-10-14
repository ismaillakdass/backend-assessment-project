using Assessment.Domain.Entities;
using Assessment.Domain.Interfaces;
using Assessment.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Assessment.Infrastructure.Repositories;

public class DonorRepository : Repository<Donor>, IDonorRepository
{
    public DonorRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Donor?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(d => d.Email.ToLower() == email.ToLower(), cancellationToken);
    }

    public async Task<Donor?> GetDonorWithDonationsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(d => d.Donations)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(string email, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(d => d.Email.ToLower() == email.ToLower());
        if (excludeId.HasValue)
        {
            query = query.Where(d => d.Id != excludeId.Value);
        }
        return await query.AnyAsync(cancellationToken);
    }
}
