using Asp.Versioning;
using Assessment.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Assessment.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/donors")]
public class DonorsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public DonorsController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// TASK 8: Pagination Problem (5 Points)
    /// This endpoint pulls the entire table of donors into memory and returns all records in a single payload.
    /// In production with tens of thousands of records, this causes high memory consumption and latency.
    /// Candidates should add server-side pagination, search/filtering, and sorting parameters.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        // ANTI-PATTERN [Task 8]:
        // Returns entire unpaginated dataset directly from DB!
        var donors = await _context.Donors
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return Ok(donors);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var donor = await _context.Donors
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

        if (donor == null) return NotFound(new { message = $"Donor {id} not found." });
        return Ok(donor);
    }
}
