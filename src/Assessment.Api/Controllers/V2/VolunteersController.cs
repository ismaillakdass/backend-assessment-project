using Asp.Versioning;
using Assessment.Application.DTOs.Volunteers;
using Assessment.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Assessment.Api.Controllers.V2;

/// <summary>
/// TASK 13: Swagger / OpenAPI Documentation (3 Points)
/// This controller is completely missing Swagger / OpenAPI documentation attributes and XML comments!
/// Notice that other controllers (e.g. V2 DonorsController) have full XML comments and [ProducesResponseType] attributes.
/// Candidates should document this controller:
/// - Add XML summary, param, and returns tags
/// - Add [ProducesResponseType] for all expected HTTP status codes (200, 201, 400, 404, 204)
/// - Specify return types for Swagger schema generation
/// </summary>
[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/volunteers")]
public class VolunteersController : ControllerBase
{
    private readonly IVolunteerService _volunteerService;

    public VolunteersController(IVolunteerService volunteerService)
    {
        _volunteerService = volunteerService;
    }

    // MISSING SWAGGER DOCUMENTATION [Task 13]: No XML docs or ProducesResponseType
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var volunteers = await _volunteerService.GetAllAsync(cancellationToken);
        return Ok(volunteers);
    }

    // MISSING SWAGGER DOCUMENTATION [Task 13]: No XML docs or ProducesResponseType
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var volunteer = await _volunteerService.GetByIdAsync(id, cancellationToken);
        if (volunteer == null) return NotFound();
        return Ok(volunteer);
    }

    // MISSING SWAGGER DOCUMENTATION [Task 13]: No XML docs or ProducesResponseType
    [HttpGet("by-city/{city}")]
    public async Task<IActionResult> GetByCity(string city, CancellationToken cancellationToken)
    {
        var volunteers = await _volunteerService.GetByCityAsync(city, cancellationToken);
        return Ok(volunteers);
    }

    // MISSING SWAGGER DOCUMENTATION [Task 13]: No XML docs or ProducesResponseType
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateVolunteerDto dto, CancellationToken cancellationToken)
    {
        var created = await _volunteerService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { version = "2.0", id = created.Id }, created);
    }

    // MISSING SWAGGER DOCUMENTATION [Task 13]: No XML docs or ProducesResponseType
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateVolunteerDto dto, CancellationToken cancellationToken)
    {
        var updated = await _volunteerService.UpdateAsync(id, dto, cancellationToken);
        return Ok(updated);
    }

    // MISSING SWAGGER DOCUMENTATION [Task 13]: No XML docs or ProducesResponseType
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _volunteerService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
