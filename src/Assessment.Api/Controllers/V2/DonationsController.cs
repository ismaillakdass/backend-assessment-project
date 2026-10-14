using System.Security.Claims;
using Asp.Versioning;
using Assessment.Application.Common;
using Assessment.Application.DTOs.Donations;
using Assessment.Application.Interfaces;
using Assessment.Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Assessment.Api.Controllers.V2;

[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/donations")]
public class DonationsController : ControllerBase
{
    private readonly IDonationService _donationService;

    public DonationsController(IDonationService donationService)
    {
        _donationService = donationService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<DonationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaged(
        [FromQuery] DonationFilterDto filter,
        [FromQuery] PaginationFilter pagination,
        CancellationToken cancellationToken)
    {
        var result = await _donationService.GetPagedAsync(filter, pagination, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(DonationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var donation = await _donationService.GetByIdAsync(id, cancellationToken);
        if (donation == null) return NotFound(new { message = $"Donation {id} not found." });
        return Ok(donation);
    }

    /// <summary>
    /// TASK 1: Legacy Endpoint Refactoring and Migration to V2 (50 Points)
    /// 
    /// In V1 (api/v1/donations/legacy-process), the donation creation logic was implemented poorly inside the controller.
    /// In this V2 endpoint, candidates must implement the clean, modern architecture:
    /// 1. Accept CreateDonationDto.
    /// 2. Validate request using FluentValidation (CreateDonationDtoValidator).
    /// 3. Delegate business operations to IDonationService / UnitOfWork.
    /// 4. Map entities via AutoMapper.
    /// 5. Execute operations asynchronously and safely within a transaction.
    /// 6. Return standard 201 Created with CreatedAtAction (Location header) and ApiResponse wrapper.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<DonationDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateDonationDto dto, CancellationToken cancellationToken)
    {
        // TODO [Task 1]: Implement the refactored V2 donation creation endpoint here.
        // Currently returns 501 Not Implemented to indicate candidate action is required.
        // Once implemented, call _donationService.CreateDonationAsync(dto, ...) and return CreatedAtAction.
        return StatusCode(StatusCodes.Status501NotImplemented, new
        {
            message = "Task 1: V2 Donation creation endpoint has not been implemented yet. " +
                      "Please refactor the legacy implementation from V1 according to the task description."
        });
    }

    /// <summary>
    /// TASK 3: Authorization / Permission System (15 Points)
    /// 
    /// This endpoint must only be accessible to users who possess the 'donations.approve' permission.
    /// Currently, the underlying PermissionAuthorizationHandler has an incomplete implementation
    /// that only grants access to Admin and fails to properly verify DB role permissions for Operator.
    /// </summary>
    [HttpPost("{id}/approve")]
    [Authorize]
    [HasPermission(Permissions.Donations.Approve)]
    [ProducesResponseType(typeof(DonationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Approve(
        int id,
        [FromBody] DonationApprovalDto dto,
        CancellationToken cancellationToken)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";
        var approved = await _donationService.ApproveDonationAsync(id, dto, currentUserId, cancellationToken);
        return Ok(approved);
    }

    /// <summary>
    /// TASK 4: Bulk Update Problem (10 Points)
    /// 
    /// This endpoint initiates a bulk status update.
    /// The underlying service method iterates through records in a foreach loop and calls SaveChanges individually.
    /// Candidates should optimize the bulk update operation using EF Core 9 ExecuteUpdateAsync.
    /// </summary>
    [HttpPost("bulk-status-update")]
    [Authorize]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> BulkUpdateStatus(
        [FromBody] BulkUpdateDonationStatusDto dto,
        CancellationToken cancellationToken)
    {
        var affectedRows = await _donationService.BulkUpdateStatusAsync(dto, cancellationToken);
        return Ok(new { affectedRows, message = $"Bulk update finished successfully. {affectedRows} donations updated." });
    }
}
