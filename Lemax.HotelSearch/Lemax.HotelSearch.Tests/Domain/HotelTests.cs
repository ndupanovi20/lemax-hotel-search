using Lemax.HotelSearch.Domain.Entities;
using Lemax.HotelSearch.Domain.ValueObjects;

namespace Lemax.HotelSearch.Tests.Domain;

public sealed class HotelTests
{
    [Fact]
    public void Constructor_ShouldCreateHotel_WhenDataIsValid()
    {
        var location = new GeoLocation(45.8150, 15.9819);

        var hotel = new Hotel(Guid.NewGuid(), "Hotel Zagreb", 100, location);

        Assert.Equal("Hotel Zagreb", hotel.Name);
        Assert.Equal(100, hotel.Price);
        Assert.Equal(location, hotel.Location);
    }

    [Fact]
    public void Constructor_ShouldTrimHotelName()
    {
        var location = new GeoLocation(45.8150, 15.9819);

        var hotel = new Hotel(Guid.NewGuid(), "  Hotel Zagreb  ", 100, location);

        Assert.Equal("Hotel Zagreb", hotel.Name);
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenIdIsEmpty()
    {
        var location = new GeoLocation(45.8150, 15.9819);

        Assert.Throws<ArgumentException>(() =>
            new Hotel(Guid.Empty, "Hotel Zagreb", 100, location));
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenNameIsEmpty()
    {
        var location = new GeoLocation(45.8150, 15.9819);

        Assert.Throws<ArgumentException>(() =>
            new Hotel(Guid.NewGuid(), "", 100, location));
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenPriceIsZeroOrLess()
    {
        var location = new GeoLocation(45.8150, 15.9819);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Hotel(Guid.NewGuid(), "Hotel Zagreb", 0, location));

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Hotel(Guid.NewGuid(), "Hotel Zagreb", -10, location));
    }

    [Fact]
    public void Update_ShouldUpdateHotel_WhenDataIsValid()
    {
        var hotel = new Hotel(
            Guid.NewGuid(),
            "Hotel Zagreb",
            100,
            new GeoLocation(45.8150, 15.9819));

        var newLocation = new GeoLocation(43.5081, 16.4402);

        hotel.Update("Hotel Split", 150, newLocation);

        Assert.Equal("Hotel Split", hotel.Name);
        Assert.Equal(150, hotel.Price);
        Assert.Equal(newLocation, hotel.Location);
    }
}