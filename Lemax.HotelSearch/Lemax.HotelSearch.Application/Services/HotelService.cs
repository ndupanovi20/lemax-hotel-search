using Lemax.HotelSearch.Application.Abstractions;
using Lemax.HotelSearch.Application.Dtos;
using Lemax.HotelSearch.Domain.Entities;
using Lemax.HotelSearch.Domain.ValueObjects;

namespace Lemax.HotelSearch.Application.Services;

public sealed class HotelService : IHotelService
{
    private readonly IHotelRepository _hotelRepository;

    public HotelService(IHotelRepository hotelRepository)
    {
        _hotelRepository = hotelRepository ?? throw new ArgumentNullException(nameof(hotelRepository));
    }

    public async Task<IReadOnlyCollection<HotelResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var hotels = await _hotelRepository.GetAllAsync(cancellationToken);

        return hotels
            .Select(MapToResponse)
            .ToList();
    }

    public async Task<HotelResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var hotel = await _hotelRepository.GetByIdAsync(id, cancellationToken);

        if (hotel is null)
        {
            return null;
        }

        return MapToResponse(hotel);
    }

    public async Task<HotelResponse> CreateAsync(CreateHotelRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var location = new GeoLocation(request.Latitude, request.Longitude);

        var hotel = new Hotel(
            Guid.NewGuid(),
            request.Name,
            request.Price,
            location);

        await _hotelRepository.AddAsync(hotel, cancellationToken);

        return MapToResponse(hotel);
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateHotelRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var hotel = await _hotelRepository.GetByIdAsync(id, cancellationToken);

        if (hotel is null)
        {
            return false;
        }

        var location = new GeoLocation(request.Latitude, request.Longitude);

        hotel.Update(request.Name, request.Price, location);

        await _hotelRepository.UpdateAsync(hotel, cancellationToken);

        return true;
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _hotelRepository.DeleteAsync(id, cancellationToken);
    }

    private static HotelResponse MapToResponse(Hotel hotel)
    {
        return new HotelResponse
        {
            Id = hotel.Id,
            Name = hotel.Name,
            Price = hotel.Price,
            Latitude = hotel.Location.Latitude,
            Longitude = hotel.Location.Longitude
        };
    }
}