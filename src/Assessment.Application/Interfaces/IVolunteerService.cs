using Assessment.Application.DTOs.Volunteers;

namespace Assessment.Application.Interfaces;

public interface IVolunteerService
{
    Task<VolunteerDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<VolunteerDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<VolunteerDto>> GetByCityAsync(string city, CancellationToken cancellationToken = default);
    Task<VolunteerDto> CreateAsync(CreateVolunteerDto dto, CancellationToken cancellationToken = default);
    Task<VolunteerDto> UpdateAsync(int id, UpdateVolunteerDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
