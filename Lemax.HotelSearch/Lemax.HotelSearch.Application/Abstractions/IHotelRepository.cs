using Lemax.HotelSearch.Domain.Entities;

namespace Lemax.HotelSearch.Application.Abstractions;

public interface IHotelRepository
{
    Task<IReadOnlyCollection<Hotel>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Hotel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(Hotel hotel, CancellationToken cancellationToken = default);

    Task UpdateAsync(Hotel hotel, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}