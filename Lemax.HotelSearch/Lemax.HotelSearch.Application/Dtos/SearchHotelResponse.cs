namespace Lemax.HotelSearch.Application.Dtos;

public sealed class SearchHotelResponse
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public double DistanceInKm { get; set; }
}