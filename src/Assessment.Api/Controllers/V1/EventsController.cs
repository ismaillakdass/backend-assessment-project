using Asp.Versioning;
using Assessment.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Assessment.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/events")]
public class EventsController : ControllerBase
{
    private readonly IEventService _eventService;

    public EventsController(IEventService eventService)
    {
        _eventService = eventService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var events = await _eventService.GetAllAsync(cancellationToken);
        return Ok(events);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var ev = await _eventService.GetByIdAsync(id, cancellationToken);
        if (ev == null) return NotFound();
        return Ok(ev);
    }

    [HttpGet("upcoming")]
    public async Task<IActionResult> GetUpcoming(CancellationToken cancellationToken)
    {
        var events = await _eventService.GetUpcomingEventsAsync(cancellationToken);
        return Ok(events);
    }
}
