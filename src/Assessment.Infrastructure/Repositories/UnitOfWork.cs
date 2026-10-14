using System.Collections.Concurrent;
using Assessment.Domain.Common;
using Assessment.Domain.Interfaces;
using Assessment.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Storage;

namespace Assessment.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _currentTransaction;
    private readonly ConcurrentDictionary<Type, object> _repositories = new();

    private IDonorRepository? _donors;
    private IDonationRepository? _donations;
    private IVolunteerRepository? _volunteers;
    private IEventRepository? _events;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public IDonorRepository Donors => _donors ??= new DonorRepository(_context);
    public IDonationRepository Donations => _donations ??= new DonationRepository(_context);
    public IVolunteerRepository Volunteers => _volunteers ??= new VolunteerRepository(_context);
    public IEventRepository Events => _events ??= new EventRepository(_context);

    public IRepository<T> Repository<T>() where T : BaseEntity
    {
        return (IRepository<T>)_repositories.GetOrAdd(typeof(T), _ => new Repository<T>(_context));
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null)
        {
            return;
        }

        _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            if (_currentTransaction != null)
            {
                await _currentTransaction.CommitAsync(cancellationToken);
            }
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.RollbackAsync(cancellationToken);
            }
        }
        finally
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }
    }

    public void Dispose()
    {
        _currentTransaction?.Dispose();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
