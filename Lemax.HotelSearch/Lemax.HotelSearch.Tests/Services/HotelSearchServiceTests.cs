using Lemax.HotelSearch.Application.Abstractions;
using Lemax.HotelSearch.Application.Dtos;
using Lemax.HotelSearch.Application.Services;
using Lemax.HotelSearch.Domain.Entities;
using Lemax.HotelSearch.Domain.ValueObjects;

namespace Lemax.HotelSearch.Tests.Services;

public sealed class HotelSearchServiceTests
{
    [Fact]
    public async Task SearchAsync_ShouldReturnHotelsUnderBudget_WhenBudgetIsProvided()
    {
        var hotels = new List<Hotel>
        {
            CreateHotel("Zagreb Budget Hotel", 80, 45.8150, 15.9819),
            CreateHotel("Zagreb Expensive Hotel", 150, 45.8150, 15.9819),
            CreateHotel("Split Budget Hotel", 70, 43.5081, 16.4402)
        };

        var service = CreateService(hotels);

        var result = await service.SearchAsync(new SearchHotelsRequest
        {
            Prompt = "I need a hotel near Zagreb under 100 euros",
            Page = 1,
            PageSize = 10
        });

        Assert.Equal(2, result.TotalCount);
        Assert.All(result.Items, hotel => Assert.True(hotel.Price <= 100));
        Assert.DoesNotContain(result.Items, hotel => hotel.Name == "Zagreb Expensive Hotel");
    }

    [Fact]
    public async Task SearchAsync_ShouldRankCheaperAndCloserHotelFirst()
    {
        var hotels = new List<Hotel>
        {
            CreateHotel("Expensive Zagreb Hotel", 150, 45.8150, 15.9819),
            CreateHotel("Cheap Zagreb Hotel", 80, 45.8151, 15.9820),
            CreateHotel("Cheap Split Hotel", 80, 43.5081, 16.4402)
        };

        var service = CreateService(hotels);

        var result = await service.SearchAsync(new SearchHotelsRequest
        {
            Prompt = "I need a hotel near Zagreb",
            Page = 1,
            PageSize = 10
        });

        Assert.Equal("Cheap Zagreb Hotel", result.Items.First().Name);
    }

    [Fact]
    public async Task SearchAsync_ShouldApplyPaging()
    {
        var hotels = new List<Hotel>
        {
            CreateHotel("Hotel 1", 50, 45.8150, 15.9819),
            CreateHotel("Hotel 2", 60, 45.8160, 15.9820),
            CreateHotel("Hotel 3", 70, 45.8170, 15.9830),
            CreateHotel("Hotel 4", 80, 45.8180, 15.9840),
            CreateHotel("Hotel 5", 90, 45.8190, 15.9850)
        };

        var service = CreateService(hotels);

        var result = await service.SearchAsync(new SearchHotelsRequest
        {
            Prompt = "I need a hotel near Zagreb",
            Page = 2,
            PageSize = 2
        });

        Assert.Equal(5, result.TotalCount);
        Assert.Equal(3, result.TotalPages);
        Assert.Equal(2, result.Page);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(2, result.Items.Count);
    }

    [Fact]
    public async Task SearchAsync_ShouldThrowException_WhenPromptHasUnsupportedLocation()
    {
        var hotels = new List<Hotel>
        {
            CreateHotel("Zagreb Hotel", 80, 45.8150, 15.9819)
        };

        var service = CreateService(hotels);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.SearchAsync(new SearchHotelsRequest
            {
                Prompt = "I need a hotel near Berlin under 100 euros",
                Page = 1,
                PageSize = 10
            }));
    }

    [Fact]
    public async Task SearchAsync_ShouldExtractBudgetFromMaxKeyword()
    {
        var hotels = new List<Hotel>
        {
            CreateHotel("Very Cheap Hotel", 55, 45.8150, 15.9819),
            CreateHotel("Regular Hotel", 70, 45.8150, 15.9819)
        };

        var service = CreateService(hotels);

        var result = await service.SearchAsync(new SearchHotelsRequest
        {
            Prompt = "Find hotel near Zagreb max 60 euros",
            Page = 1,
            PageSize = 10
        });

        Assert.Single(result.Items);
        Assert.Equal("Very Cheap Hotel", result.Items.First().Name);
    }

    private static HotelSearchService CreateService(IReadOnlyCollection<Hotel> hotels)
    {
        return new HotelSearchService(new FakeHotelRepository(hotels));
    }

    private static Hotel CreateHotel(string name, decimal price, double latitude, double longitude)
    {
        return new Hotel(
            Guid.NewGuid(),
            name,
            price,
            new GeoLocation(latitude, longitude));
    }

    private sealed class FakeHotelRepository : IHotelRepository
    {
        private readonly IReadOnlyCollection<Hotel> _hotels;

        public FakeHotelRepository(IReadOnlyCollection<Hotel> hotels)
        {
            _hotels = hotels;
        }

        public Task<IReadOnlyCollection<Hotel>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_hotels);
        }

        public Task<Hotel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var hotel = _hotels.FirstOrDefault(hotel => hotel.Id == id);

            return Task.FromResult(hotel);
        }

        public Task AddAsync(Hotel hotel, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Hotel hotel, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(false);
        }
    }
}