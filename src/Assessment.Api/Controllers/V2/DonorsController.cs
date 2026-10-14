using Asp.Versioning;
using Assessment.Application.Common;
using Assessment.Application.DTOs.Donors;
using Assessment.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Assessment.Api.Controllers.V2;

[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/donors")]
public class DonorsController : ControllerBase
{
    private readonly IDonorService _donorService;

    public DonorsController(IDonorService donorService)
    {
        _donorService = donorService;
    }

    /// <summary>
    /// Gets a paginated list of donors with optional search and sorting.
    /// </summary>
    /// <param name="filter">Pagination, search and sorting parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paginated donors list.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<DonorDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaged([FromQuery] PaginationFilter filter, CancellationToken cancellationToken)
    {
        var result = await _donorService.GetPagedAsync(filter, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets donor details including donation statistics.
    /// </summary>
    /// <param name="id">The donor identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Donor detail information.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(DonorDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var donor = await _donorService.GetDonorDetailsAsync(id, cancellationToken);
        if (donor == null) return NotFound(new { message = $"Donor with ID {id} not found." });
        return Ok(donor);
    }

    /// <summary>
    /// Creates a new donor.
    /// </summary>
    /// <param name="dto">Donor creation data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The newly created donor.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(DonorDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateDonorDto dto, CancellationToken cancellationToken)
    {
        var created = await _donorService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { version = "2.0", id = created.Id }, created);
    }

    /// <summary>
    /// Updates an existing donor.
    /// </summary>
    /// <param name="id">Donor ID to update.</param>
    /// <param name="dto">Updated donor information.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated donor.</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(DonorDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDonorDto dto, CancellationToken cancellationToken)
    {
        var updated = await _donorService.UpdateAsync(id, dto, cancellationToken);
        return Ok(updated);
    }

    /// <summary>
    /// Soft deletes a donor.
    /// </summary>
    /// <param name="id">Donor ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _donorService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
