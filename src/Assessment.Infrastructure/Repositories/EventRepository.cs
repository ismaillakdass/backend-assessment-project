using Assessment.Domain.Entities;
using Assessment.Domain.Interfaces;
using Assessment.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Assessment.Infrastructure.Repositories;

public class EventRepository : Repository<Event>, IEventRepository
{
    public EventRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Event>> GetUpcomingEventsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(e => e.EventVolunteers)
            .Where(e => e.EventDate >= DateTime.UtcNow && e.IsActive)
            .OrderBy(e => e.EventDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<Event?> GetEventWithVolunteersAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(e => e.EventVolunteers)
            .ThenInclude(ev => ev.Volunteer)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }
}
