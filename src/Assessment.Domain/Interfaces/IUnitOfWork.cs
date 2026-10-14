namespace Assessment.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IDonorRepository Donors { get; }
    IDonationRepository Donations { get; }
    IVolunteerRepository Volunteers { get; }
    IEventRepository Events { get; }
    IRepository<T> Repository<T>() where T : Common.BaseEntity;

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
