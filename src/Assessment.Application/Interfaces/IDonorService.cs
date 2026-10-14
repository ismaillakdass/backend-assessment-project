using Assessment.Application.Common;
using Assessment.Application.DTOs.Donors;

namespace Assessment.Application.Interfaces;

public interface IDonorService
{
    Task<DonorDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<DonorDetailDto?> GetDonorDetailsAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DonorDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PagedResult<DonorDto>> GetPagedAsync(PaginationFilter filter, CancellationToken cancellationToken = default);
    Task<DonorDto> CreateAsync(CreateDonorDto dto, CancellationToken cancellationToken = default);
    Task<DonorDto> UpdateAsync(int id, UpdateDonorDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
