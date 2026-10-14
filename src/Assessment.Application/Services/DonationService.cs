using AutoMapper;
using Assessment.Application.Common;
using Assessment.Application.DTOs.Donations;
using Assessment.Application.Interfaces;
using Assessment.Domain.Entities;
using Assessment.Domain.Enums;
using Assessment.Domain.Exceptions;
using Assessment.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Assessment.Application.Services;

public class DonationService : IDonationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<DonationService> _logger;

    public DonationService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<DonationService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<DonationDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var donation = await _unitOfWork.Donations.GetDonationWithDonorAsync(id, cancellationToken);
        return donation == null ? null : _mapper.Map<DonationDto>(donation);
    }

    public async Task<IReadOnlyList<DonationDto>> GetByDonorIdAsync(int donorId, CancellationToken cancellationToken = default)
    {
        var donations = await _unitOfWork.Donations.GetByDonorIdAsync(donorId, cancellationToken);
        return _mapper.Map<IReadOnlyList<DonationDto>>(donations);
    }

    public async Task<PagedResult<DonationDto>> GetPagedAsync(
        DonationFilterDto filter,
        PaginationFilter pagination,
        CancellationToken cancellationToken = default)
    {
        var query = _unitOfWork.Donations.Query()
            .Include(d => d.Donor)
            .AsNoTracking();

        if (filter.DonorId.HasValue)
            query = query.Where(d => d.DonorId == filter.DonorId.Value);

        if (filter.Status.HasValue)
            query = query.Where(d => d.Status == filter.Status.Value);

        if (filter.DonationType.HasValue)
            query = query.Where(d => d.DonationType == filter.DonationType.Value);

        if (filter.FromDate.HasValue)
            query = query.Where(d => d.DonationDate >= filter.FromDate.Value);

        if (filter.ToDate.HasValue)
            query = query.Where(d => d.DonationDate <= filter.ToDate.Value);

        if (filter.MinAmount.HasValue)
            query = query.Where(d => d.Amount >= filter.MinAmount.Value);

        if (filter.MaxAmount.HasValue)
            query = query.Where(d => d.Amount <= filter.MaxAmount.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        query = pagination.SortBy?.ToLower() switch
        {
            "amount" => pagination.IsAscending ? query.OrderBy(d => d.Amount) : query.OrderByDescending(d => d.Amount),
            "date" or "donationdate" => pagination.IsAscending ? query.OrderBy(d => d.DonationDate) : query.OrderByDescending(d => d.DonationDate),
            _ => query.OrderByDescending(d => d.DonationDate)
        };

        var items = await query
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = _mapper.Map<IReadOnlyList<DonationDto>>(items);
        return new PagedResult<DonationDto>(dtos, totalCount, pagination.PageNumber, pagination.PageSize);
    }

    /// <summary>
    /// TASK 5: Transaction Management Problem (10 Points)
    /// Multiple database operations are executed and committed independently.
    /// If updating the donor's total amount or creating the audit log fails,
    /// the donation has already been persisted, leaving the database in an inconsistent state.
    /// </summary>
    public async Task<DonationDto> CreateDonationAsync(
        CreateDonationDto dto,
        string? currentUserId = null,
        CancellationToken cancellationToken = default)
    {
        var donor = await _unitOfWork.Donors.GetByIdAsync(dto.DonorId, cancellationToken);
        if (donor == null)
        {
            throw new NotFoundException(nameof(Donor), dto.DonorId);
        }

        if (!donor.IsActive)
        {
            throw new BusinessException($"Cannot create donation for inactive donor {dto.DonorId}.", "INACTIVE_DONOR");
        }
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {


            // Operation 1: Create Donation
            var donation = _mapper.Map<Donation>(dto);
            donation.Status = DonationStatus.Pending;
            donation.CreatedAt = DateTime.UtcNow;

            await _unitOfWork.Donations.AddAsync(donation, cancellationToken);
            // PROBLEM [Task 5]: First SaveChanges commits donation without transaction!
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Operation 2: Update Donor Total Donated Amount
            donor.TotalDonatedAmount += donation.Amount;
            donor.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Donors.UpdateAsync(donor, cancellationToken);
            // PROBLEM [Task 5]: Second SaveChanges committed independently!
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Operation 3: Audit Log
            var audit = new AuditLog
            {
                EntityName = nameof(Donation),
                Action = "Create",
                Timestamp = DateTime.UtcNow,
                Details = $"Donation created with amount {donation.Amount} for Donor {donor.Id}",
                UserId = currentUserId
            };
            await _unitOfWork.Repository<AuditLog>().AddAsync(audit, cancellationToken);
            // PROBLEM [Task 5]: Third SaveChanges committed independently!
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken); // son SaveChanges + commit

            _logger.LogInformation("Successfully created donation {DonationId}", donation.Id);
            return _mapper.Map<DonationDto>(donation);
        }
        catch (System.Exception)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);

            throw;
        }
    }

    /// <summary>
    /// TASK 10: Logging & Sensitive Data Problem (5 Points)
    /// Uses un-structured string interpolation instead of semantic structured logging.
    /// Exposes sensitive customer details (donor phone, email) in plain application logs.
    /// Exception handling swallows or mishandles stack trace context.
    /// </summary>
    public async Task<DonationDto> ApproveDonationAsync(
        int id,
        DonationApprovalDto dto,
        string approvedByUserId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var donation = await _unitOfWork.Donations.GetDonationWithDonorAsync(id, cancellationToken);
            if (donation == null)
            {
                throw new NotFoundException(nameof(Donation), id);
            }

            if (donation.Status != DonationStatus.Pending)
            {
                throw new BusinessException($"Donation {id} is not in Pending status. Current: {donation.Status}", "INVALID_STATUS");
            }

            donation.Status = DonationStatus.Approved;
            donation.ApprovedByUserId = approvedByUserId;
            donation.ApprovedAt = DateTime.UtcNow;
            if (!string.IsNullOrWhiteSpace(dto.Notes))
            {
                donation.Notes = string.IsNullOrWhiteSpace(donation.Notes) ? dto.Notes : $"{donation.Notes}; {dto.Notes}";
            }

            await _unitOfWork.Donations.UpdateAsync(donation, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // ANTI-PATTERN [Task 10]:
            // 1. String interpolation breaks log aggregation in tools like Elasticsearch/Seq/Datadog.
            // 2. Sensitive personal data (Email, Phone) logged into plain text!
            // 3. Lack of structured parameters like {DonationId}, {ApprovedByUserId}.
            _logger.LogInformation($"[APPROVAL] Donation {id} approved by user {approvedByUserId} for donor {donation.Donor?.FirstName} {donation.Donor?.LastName} (Email: {donation.Donor?.Email}, Phone: {donation.Donor?.Phone}) with amount {donation.Amount}");

            return _mapper.Map<DonationDto>(donation);
        }
        catch (Exception ex)
        {
            // ANTI-PATTERN [Task 10]: String concatenation loses inner exception context and stack trace structure
            _logger.LogError("Error in ApproveDonationAsync: " + ex.Message);
            throw;
        }
    }

    /// <summary>
    /// TASK 4: Bulk Update Problem (10 Points)
    /// Iterates through hundreds of records, tracking all in memory and updating them one-by-one.
    /// Candidates should optimize this using EF Core 9 ExecuteUpdateAsync.
    /// </summary>
    public async Task<int> BulkUpdateStatusAsync(BulkUpdateDonationStatusDto dto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting bulk update from {CurrentStatus} to {NewStatus}", dto.CurrentStatus, dto.NewStatus);

        var affectedRows = await _unitOfWork.Donations.BulkUpdateStatusAsync(
            dto.CurrentStatus,
            dto.NewStatus,
            cancellationToken);

        _logger.LogInformation("Bulk update finished. Affected rows: {Count}", affectedRows);
        return affectedRows;
    }

    public async Task<int> CancelPendingDonationsOlderThanDaysAsync(int days, CancellationToken cancellationToken = default)
    {
        var cutoff = DateTime.UtcNow.AddDays(-days);
        var pending = await _unitOfWork.Donations.Query()
            .Where(d => d.Status == DonationStatus.Pending && d.DonationDate < cutoff)
            .ToListAsync(cancellationToken);

        foreach (var donation in pending)
        {
            donation.Status = DonationStatus.Cancelled;
            donation.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Donations.UpdateAsync(donation, cancellationToken);
        }

        return await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
