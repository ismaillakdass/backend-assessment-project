using Assessment.Domain.Entities;
using Assessment.Domain.Interfaces;
using Assessment.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Assessment.Infrastructure.Repositories;

public class VolunteerRepository : Repository<Volunteer>, IVolunteerRepository
{
    public VolunteerRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Volunteer>> GetVolunteersByCityAsync(string city, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(v => v.City.ToLower() == city.ToLower() && v.IsActive)
            .OrderBy(v => v.LastName)
            .ThenBy(v => v.FirstName)
            .ToListAsync(cancellationToken);
    }

    public async Task<Volunteer?> GetVolunteerWithEventsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(v => v.EventVolunteers)
            .ThenInclude(ev => ev.Event)
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
    }
}
