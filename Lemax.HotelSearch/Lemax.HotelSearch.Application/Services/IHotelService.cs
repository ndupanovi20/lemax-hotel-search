using Lemax.HotelSearch.Application.Dtos;

namespace Lemax.HotelSearch.Application.Services;

public interface IHotelService
{
    Task<IReadOnlyCollection<HotelResponse>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<HotelResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<HotelResponse> CreateAsync(CreateHotelRequest request, CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(Guid id, UpdateHotelRequest request, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}