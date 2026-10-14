using Asp.Versioning;
using Assessment.Domain.Entities;
using Assessment.Domain.Enums;
using Assessment.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Assessment.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/donations")]
public class DonationsController : ControllerBase
{
    // ANTI-PATTERN [Task 1]: Direct DbContext injection into controller bypassing service layer and repository/UoW!
    private readonly ApplicationDbContext _context;

    public DonationsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        // Bad practice: returns raw entities with change tracking enabled
        var donations = await _context.Donations.ToListAsync();
        return Ok(donations);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var donation = await _context.Donations.FindAsync(id);
        if (donation == null) return NotFound();
        return Ok(donation);
    }

    /// <summary>
    /// TASK 1: Legacy Endpoint Refactoring and Migration to V2 (50 Points)
    /// 
    /// This legacy endpoint is filled with severe real-world anti-patterns:
    /// 1. Direct DbContext injection in Controller
    /// 2. Business logic written directly inside controller action
    /// 3. Inefficient LINQ and N+1 queries (looping queries against DB)
    /// 4. Change tracking enabled everywhere without AsNoTracking
    /// 5. No FluentValidation or DTO validation (raw ad-hoc object accepted)
    /// 6. Wrong HTTP status codes (returns 200 OK instead of 201 Created with Location header)
    /// 7. Raw EF Core entity returned directly as response, exposing database internals
    /// 8. No database transaction management across multiple mutations
    /// 9. Repetitive try/catch swallowing or leaking raw exception details
    /// 10. Synchronous or poorly managed EF operations
    /// 
    /// Candidates are expected to refactor this functionality completely
    /// and implement it cleanly under /api/v2/donations using:
    /// - Service layer (IDonationService)
    /// - Repository and Unit of Work pattern
    /// - Proper DTOs
    /// - FluentValidation
    /// - AutoMapper
    /// - Async EF Core
    /// - Proper transaction management
    /// - Proper RESTful response (201 Created with Location header)
    /// </summary>
    [HttpPost("legacy-process")]
    public async Task<IActionResult> LegacyProcessDonation([FromBody] LegacyDonationRequest request)
    {
        try
        {
            // FLAW: Business logic inside controller
            if (request.DonorId <= 0)
            {
                // Wrong status code & inconsistent response
                return StatusCode(400, "Invalid donor id");
            }

            // FLAW: Inefficient query with full tracking
            var donor = await _context.Donors.FirstOrDefaultAsync(d => d.Id == request.DonorId);
            if (donor == null)
            {
                return StatusCode(404, "Donor not found in database.");
            }

            if (!donor.IsActive)
            {
                return StatusCode(400, "Donor account is disabled.");
            }

            // FLAW: Missing amount validation (zero or negative allowed)
            // N+1 Query: queries existing donations to check donor history in a bad way
            var previousDonationsCount = _context.Donations.Where(d => d.DonorId == request.DonorId).Count();

            // Create donation entity manually
            var donation = new Donation
            {
                DonorId = request.DonorId,
                Amount = request.Amount,
                DonationType = (DonationType)request.DonationType,
                DonationDate = request.DonationDate == default ? DateTime.UtcNow : request.DonationDate,
                Status = DonationStatus.Pending,
                Notes = $"Legacy entry. Previous donations: {previousDonationsCount}. Notes: {request.Notes}",
                CreatedAt = DateTime.UtcNow
            };

            _context.Donations.Add(donation);
            // Save step 1 without transaction!
            await _context.SaveChangesAsync();

            // Update donor stats manually in controller
            donor.TotalDonatedAmount += donation.Amount;
            donor.UpdatedAt = DateTime.UtcNow;
            _context.Donors.Update(donor);
            // Save step 2 without transaction!
            await _context.SaveChangesAsync();

            // Write audit log manually in controller
            var audit = new AuditLog
            {
                EntityName = "Donation",
                Action = "LegacyCreate",
                Timestamp = DateTime.UtcNow,
                Details = $"Legacy created donation {donation.Id} for amount {donation.Amount}"
            };
            _context.AuditLogs.Add(audit);
            await _context.SaveChangesAsync();

            // FLAW: Returns 200 OK with raw EF entity (exposing database internals & potential circular reference)
            // instead of 201 Created with Location header and DTO!
            return Ok(donation);
        }
        catch (Exception ex)
        {
            // FLAW: Raw exception message returned to client
            return StatusCode(500, new { error = ex.Message, stack = ex.StackTrace });
        }
    }
}

public class LegacyDonationRequest
{
    public int DonorId { get; set; }
    public decimal Amount { get; set; }
    public int DonationType { get; set; } = 1;
    public DateTime DonationDate { get; set; }
    public string? Notes { get; set; }
}
