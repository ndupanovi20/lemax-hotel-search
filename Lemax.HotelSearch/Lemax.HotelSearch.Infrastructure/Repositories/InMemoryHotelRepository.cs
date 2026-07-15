using System.Collections.Concurrent;
using Lemax.HotelSearch.Application.Abstractions;
using Lemax.HotelSearch.Domain.Entities;

namespace Lemax.HotelSearch.Infrastructure.Repositories;

public sealed class InMemoryHotelRepository : IHotelRepository
{
    private readonly ConcurrentDictionary<Guid, Hotel> _hotels = new();

    public Task<IReadOnlyCollection<Hotel>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<Hotel> hotels = _hotels.Values.ToList();

        return Task.FromResult(hotels);
    }

    public Task<Hotel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _hotels.TryGetValue(id, out var hotel);

        return Task.FromResult(hotel);
    }

    public Task AddAsync(Hotel hotel, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(hotel);

        var added = _hotels.TryAdd(hotel.Id, hotel);

        if (!added)
        {
            throw new InvalidOperationException($"Hotel with id '{hotel.Id}' already exists.");
        }

        return Task.CompletedTask;
    }

    public Task UpdateAsync(Hotel hotel, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(hotel);

        _hotels[hotel.Id] = hotel;

        return Task.CompletedTask;
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var deleted = _hotels.TryRemove(id, out _);

        return Task.FromResult(deleted);
    }
}