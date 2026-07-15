using Lemax.HotelSearch.Domain.ValueObjects;

namespace Lemax.HotelSearch.Tests.Domain;

public sealed class GeoLocationTests
{
    [Fact]
    public void Constructor_ShouldCreateGeoLocation_WhenCoordinatesAreValid()
    {
        var location = new GeoLocation(45.8150, 15.9819);

        Assert.Equal(45.8150, location.Latitude);
        Assert.Equal(15.9819, location.Longitude);
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenLatitudeIsInvalid()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new GeoLocation(100, 15.9819));
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenLongitudeIsInvalid()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new GeoLocation(45.8150, 200));
    }
}