
using Lemax.HotelSearch.Domain.ValueObjects;
namespace Lemax.HotelSearch.Domain.Entities;

public sealed class Hotel
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public decimal Price { get; private set; }
    public GeoLocation Location { get; private set; }

    public Hotel(Guid id, string name, decimal price, GeoLocation location)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Hotel id cannot be empty.", nameof(id));
        }

        Id = id;
        Name = ValidateName(name);
        Price = ValidatePrice(price);
        Location = location ?? throw new ArgumentNullException(nameof(location));
    }

    public void Update(string name, decimal price, GeoLocation location)
    {
        Name = ValidateName(name);
        Price = ValidatePrice(price);
        Location = location ?? throw new ArgumentNullException(nameof(location));
    }

    private static string ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Hotel name is required.", nameof(name));
        }

        return name.Trim();
    }

    private static decimal ValidatePrice(decimal price)
    {
        if (price <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(price), "Hotel price must be greater than zero.");
        }

        return price;
    }
}
