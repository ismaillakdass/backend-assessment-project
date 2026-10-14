using AutoMapper;
using Assessment.Application.DTOs.Events;
using Assessment.Application.Interfaces;
using Assessment.Domain.Entities;
using Assessment.Domain.Exceptions;
using Assessment.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Assessment.Application.Services;

public class EventService : IEventService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<EventService> _logger;

    public EventService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<EventService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<EventDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var ev = await _unitOfWork.Events.GetEventWithVolunteersAsync(id, cancellationToken);
        return ev == null ? null : _mapper.Map<EventDto>(ev);
    }

    public async Task<IReadOnlyList<EventDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var events = await _unitOfWork.Events.Query()
            .Include(e => e.EventVolunteers)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return _mapper.Map<IReadOnlyList<EventDto>>(events);
    }

    public async Task<IReadOnlyList<EventDto>> GetUpcomingEventsAsync(CancellationToken cancellationToken = default)
    {
        var events = await _unitOfWork.Events.GetUpcomingEventsAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<EventDto>>(events);
    }

    public async Task<EventDto> CreateAsync(CreateEventDto dto, CancellationToken cancellationToken = default)
    {
        var ev = _mapper.Map<Event>(dto);
        ev.IsActive = true;
        ev.CreatedAt = DateTime.UtcNow;

        await _unitOfWork.Events.AddAsync(ev, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<EventDto>(ev);
    }

    public async Task<bool> AssignVolunteerAsync(int eventId, int volunteerId, CancellationToken cancellationToken = default)
    {
        var ev = await _unitOfWork.Events.GetEventWithVolunteersAsync(eventId, cancellationToken);
        if (ev == null)
        {
            throw new NotFoundException(nameof(Event), eventId);
        }

        var volunteer = await _unitOfWork.Volunteers.GetByIdAsync(volunteerId, cancellationToken);
        if (volunteer == null)
        {
            throw new NotFoundException(nameof(Volunteer), volunteerId);
        }

        if (!volunteer.IsActive)
        {
            throw new BusinessException($"Volunteer {volunteerId} is inactive and cannot be assigned.", "INACTIVE_VOLUNTEER");
        }

        if (ev.EventVolunteers.Count >= ev.Capacity)
        {
            throw new BusinessException($"Event {eventId} has reached maximum capacity ({ev.Capacity}).", "EVENT_FULL");
        }

        if (ev.EventVolunteers.Any(evl => evl.VolunteerId == volunteerId))
        {
            throw new BusinessException($"Volunteer {volunteerId} is already assigned to event {eventId}.", "DUPLICATE_ASSIGNMENT");
        }

        ev.EventVolunteers.Add(new EventVolunteer
        {
            EventId = eventId,
            VolunteerId = volunteerId,
            AssignedAt = DateTime.UtcNow
        });

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
