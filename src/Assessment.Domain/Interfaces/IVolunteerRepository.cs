using Assessment.Domain.Entities;

namespace Assessment.Domain.Interfaces;

public interface IVolunteerRepository : IRepository<Volunteer>
{
    Task<IReadOnlyList<Volunteer>> GetVolunteersByCityAsync(string city, CancellationToken cancellationToken = default);
    Task<Volunteer?> GetVolunteerWithEventsAsync(int id, CancellationToken cancellationToken = default);
}
