using Assessment.Application.DTOs.Volunteers;
using Assessment.Application.Interfaces;
using Assessment.Domain.Entities;
using Assessment.Domain.Exceptions;
using Assessment.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Assessment.Application.Services;

/// <summary>
/// TASK 12: AutoMapper (3 Points)
/// This service contains repetitive, manual mapping boilerplate instead of using AutoMapper.
/// Candidates should configure AutoMapper Profile for Volunteer <-> VolunteerDto
/// and replace manual property assignments with IMapper.
/// </summary>
public class VolunteerService : IVolunteerService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<VolunteerService> _logger;

    public VolunteerService(IUnitOfWork unitOfWork, ILogger<VolunteerService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<VolunteerDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var v = await _unitOfWork.Volunteers.GetByIdAsync(id, cancellationToken);
        if (v == null) return null;

        // ANTI-PATTERN [Task 12]: Manual repetitive mapping boilerplate
        return new VolunteerDto
        {
            Id = v.Id,
            FirstName = v.FirstName,
            LastName = v.LastName,
            Email = v.Email,
            Phone = v.Phone,
            City = v.City,
            IsActive = v.IsActive,
            Skills = v.Skills,
            CreatedAt = v.CreatedAt,
            AssignedEventsCount = 0
        };
    }

    public async Task<IReadOnlyList<VolunteerDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var volunteers = await _unitOfWork.Volunteers.Query()
            .Include(v => v.EventVolunteers)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        // ANTI-PATTERN [Task 12]: Repeated manual LINQ projection
        return volunteers.Select(v => new VolunteerDto
        {
            Id = v.Id,
            FirstName = v.FirstName,
            LastName = v.LastName,
            Email = v.Email,
            Phone = v.Phone,
            City = v.City,
            IsActive = v.IsActive,
            Skills = v.Skills,
            CreatedAt = v.CreatedAt,
            AssignedEventsCount = v.EventVolunteers.Count
        }).ToList();
    }

    public async Task<IReadOnlyList<VolunteerDto>> GetByCityAsync(string city, CancellationToken cancellationToken = default)
    {
        var volunteers = await _unitOfWork.Volunteers.GetVolunteersByCityAsync(city, cancellationToken);

        // ANTI-PATTERN [Task 12]: Repetitive manual mapping
        var result = new List<VolunteerDto>();
        foreach (var v in volunteers)
        {
            result.Add(new VolunteerDto
            {
                Id = v.Id,
                FirstName = v.FirstName,
                LastName = v.LastName,
                Email = v.Email,
                Phone = v.Phone,
                City = v.City,
                IsActive = v.IsActive,
                Skills = v.Skills,
                CreatedAt = v.CreatedAt,
                AssignedEventsCount = 0
            });
        }
        return result;
    }

    public async Task<VolunteerDto> CreateAsync(CreateVolunteerDto dto, CancellationToken cancellationToken = default)
    {
        // ANTI-PATTERN [Task 12]: Manual entity creation
        var volunteer = new Volunteer
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone,
            City = dto.City,
            Skills = dto.Skills,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Volunteers.AddAsync(volunteer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ANTI-PATTERN [Task 12]: Manual mapping to DTO
        return new VolunteerDto
        {
            Id = volunteer.Id,
            FirstName = volunteer.FirstName,
            LastName = volunteer.LastName,
            Email = volunteer.Email,
            Phone = volunteer.Phone,
            City = volunteer.City,
            IsActive = volunteer.IsActive,
            Skills = volunteer.Skills,
            CreatedAt = volunteer.CreatedAt,
            AssignedEventsCount = 0
        };
    }

    public async Task<VolunteerDto> UpdateAsync(int id, UpdateVolunteerDto dto, CancellationToken cancellationToken = default)
    {
        var volunteer = await _unitOfWork.Volunteers.GetByIdAsync(id, cancellationToken);
        if (volunteer == null)
        {
            throw new NotFoundException(nameof(Volunteer), id);
        }

        volunteer.FirstName = dto.FirstName;
        volunteer.LastName = dto.LastName;
        volunteer.Phone = dto.Phone;
        volunteer.City = dto.City;
        volunteer.IsActive = dto.IsActive;
        volunteer.Skills = dto.Skills;
        volunteer.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Volunteers.UpdateAsync(volunteer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new VolunteerDto
        {
            Id = volunteer.Id,
            FirstName = volunteer.FirstName,
            LastName = volunteer.LastName,
            Email = volunteer.Email,
            Phone = volunteer.Phone,
            City = volunteer.City,
            IsActive = volunteer.IsActive,
            Skills = volunteer.Skills,
            CreatedAt = volunteer.CreatedAt,
            AssignedEventsCount = 0
        };
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var volunteer = await _unitOfWork.Volunteers.GetByIdAsync(id, cancellationToken);
        if (volunteer == null)
        {
            throw new NotFoundException(nameof(Volunteer), id);
        }

        volunteer.IsActive = false;
        volunteer.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Volunteers.UpdateAsync(volunteer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
