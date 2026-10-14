using Assessment.Domain.Entities;

namespace Assessment.Domain.Interfaces;

public interface IDonorRepository : IRepository<Donor>
{
    Task<Donor?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<Donor?> GetDonorWithDonationsAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, int? excludeId = null, CancellationToken cancellationToken = default);
}
