using AutoMapper;
using Assessment.Application.Common;
using Assessment.Application.DTOs.Donors;
using Assessment.Application.Interfaces;
using Assessment.Domain.Entities;
using Assessment.Domain.Exceptions;
using Assessment.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Assessment.Application.Services;

public class DonorService : IDonorService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<DonorService> _logger;

    public DonorService(IUnitOfWork _unitOfWork, IMapper mapper, ILogger<DonorService> logger)
    {
        this._unitOfWork = _unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<DonorDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var donor = await _unitOfWork.Donors.GetByIdAsync(id, cancellationToken);
        return donor == null ? null : _mapper.Map<DonorDto>(donor);
    }

    public async Task<DonorDetailDto?> GetDonorDetailsAsync(int id, CancellationToken cancellationToken = default)
    {
        var donor = await _unitOfWork.Donors.GetDonorWithDonationsAsync(id, cancellationToken);
        if (donor == null) return null;

        var detail = new DonorDetailDto
        {
            Id = donor.Id,
            FirstName = donor.FirstName,
            LastName = donor.LastName,
            Email = donor.Email,
            Phone = donor.Phone,
            IsActive = donor.IsActive,
            TotalDonatedAmount = donor.TotalDonatedAmount,
            CreatedAt = donor.CreatedAt,
            DonationCount = donor.Donations.Count,
            LastDonationDate = donor.Donations.OrderByDescending(d => d.DonationDate).FirstOrDefault()?.DonationDate
        };

        return detail;
    }

    public async Task<IReadOnlyList<DonorDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var donors = await _unitOfWork.Donors.GetAllAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<DonorDto>>(donors);
    }

    public async Task<PagedResult<DonorDto>> GetPagedAsync(PaginationFilter filter, CancellationToken cancellationToken = default)
    {
        var query = _unitOfWork.Donors.Query().AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var search = filter.SearchTerm.Trim().ToLower();
            query = query.Where(d => d.FirstName.ToLower().Contains(search) ||
                                     d.LastName.ToLower().Contains(search) ||
                                     d.Email.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        query = filter.SortBy?.ToLower() switch
        {
            "firstname" => filter.IsAscending ? query.OrderBy(d => d.FirstName) : query.OrderByDescending(d => d.FirstName),
            "lastname" => filter.IsAscending ? query.OrderBy(d => d.LastName) : query.OrderByDescending(d => d.LastName),
            "totaldonatedamount" => filter.IsAscending ? query.OrderBy(d => d.TotalDonatedAmount) : query.OrderByDescending(d => d.TotalDonatedAmount),
            "createdat" => filter.IsAscending ? query.OrderBy(d => d.CreatedAt) : query.OrderByDescending(d => d.CreatedAt),
            _ => query.OrderByDescending(d => d.Id)
        };

        var items = await query
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = _mapper.Map<IReadOnlyList<DonorDto>>(items);
        return new PagedResult<DonorDto>(dtos, totalCount, filter.PageNumber, filter.PageSize);
    }

    public async Task<DonorDto> CreateAsync(CreateDonorDto dto, CancellationToken cancellationToken = default)
    {
        if (await _unitOfWork.Donors.EmailExistsAsync(dto.Email, cancellationToken: cancellationToken))
        {
            throw new BusinessException($"A donor with email '{dto.Email}' already exists.", "DUPLICATE_EMAIL");
        }

        var donor = _mapper.Map<Donor>(dto);
        donor.CreatedAt = DateTime.UtcNow;
        donor.IsActive = true;
        donor.TotalDonatedAmount = 0m;

        await _unitOfWork.Donors.AddAsync(donor, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created new donor with Id: {DonorId}", donor.Id);
        return _mapper.Map<DonorDto>(donor);
    }

    public async Task<DonorDto> UpdateAsync(int id, UpdateDonorDto dto, CancellationToken cancellationToken = default)
    {
        var donor = await _unitOfWork.Donors.GetByIdAsync(id, cancellationToken);
        if (donor == null)
        {
            throw new NotFoundException(nameof(Donor), id);
        }

        donor.FirstName = dto.FirstName;
        donor.LastName = dto.LastName;
        donor.Phone = dto.Phone;
        donor.IsActive = dto.IsActive;
        donor.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Donors.UpdateAsync(donor, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<DonorDto>(donor);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var donor = await _unitOfWork.Donors.GetByIdAsync(id, cancellationToken);
        if (donor == null)
        {
            throw new NotFoundException(nameof(Donor), id);
        }

        // Soft delete
        donor.IsActive = false;
        donor.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Donors.UpdateAsync(donor, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
