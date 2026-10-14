using Assessment.Domain.Entities;

namespace Assessment.Domain.Interfaces;

public interface IEventRepository : IRepository<Event>
{
    Task<IReadOnlyList<Event>> GetUpcomingEventsAsync(CancellationToken cancellationToken = default);
    Task<Event?> GetEventWithVolunteersAsync(int id, CancellationToken cancellationToken = default);
}
