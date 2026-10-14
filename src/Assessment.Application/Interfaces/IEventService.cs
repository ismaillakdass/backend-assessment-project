using Assessment.Application.DTOs.Events;

namespace Assessment.Application.Interfaces;

public interface IEventService
{
    Task<EventDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EventDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EventDto>> GetUpcomingEventsAsync(CancellationToken cancellationToken = default);
    Task<EventDto> CreateAsync(CreateEventDto dto, CancellationToken cancellationToken = default);
    Task<bool> AssignVolunteerAsync(int eventId, int volunteerId, CancellationToken cancellationToken = default);
}
