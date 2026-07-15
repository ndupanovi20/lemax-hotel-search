using System.Globalization;
using System.Text.RegularExpressions;
using Lemax.HotelSearch.Application.Abstractions;
using Lemax.HotelSearch.Application.Dtos;
using Lemax.HotelSearch.Domain.Entities;
using Lemax.HotelSearch.Domain.ValueObjects;

namespace Lemax.HotelSearch.Application.Services;

public sealed class HotelSearchService : IHotelSearchService
{
    private static readonly Dictionary<string, GeoLocation> SupportedLocations = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Zagreb"] = new(45.8150, 15.9819),
        ["Split"] = new(43.5081, 16.4402),
        ["Dubrovnik"] = new(42.6507, 18.0944),
        ["Zadar"] = new(44.1194, 15.2314),
        ["Rijeka"] = new(45.3271, 14.4422),
        ["Pula"] = new(44.8666, 13.8496),
        ["Rovinj"] = new(45.0812, 13.6387),
        ["Osijek"] = new(45.5550, 18.6955),
        ["Plitvice"] = new(44.8800, 15.6160),
        ["Velika Gorica"] = new(45.7132, 16.0752)
    };

    private static readonly Regex BudgetWithKeywordRegex = new(
        @"\b(?:under|below|max|maximum|up to|less than|budget)\s*(?:€|eur|euros?)?\s*(\d+(?:[.,]\d+)?)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex EuroAmountRegex = new(
        @"\b(\d+(?:[.,]\d+)?)\s*(?:€|eur|euros?)\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private readonly IHotelRepository _hotelRepository;

    public HotelSearchService(IHotelRepository hotelRepository)
    {
        _hotelRepository = hotelRepository ?? throw new ArgumentNullException(nameof(hotelRepository));
    }

    public async Task<PagedResponse<SearchHotelResponse>> SearchAsync(
        SearchHotelsRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Prompt))
        {
            throw new ArgumentException("Search prompt is required.", nameof(request.Prompt));
        }

        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 10 : Math.Min(request.PageSize, 50);

        var parsedPrompt = ParsePrompt(request.Prompt);

        var hotels = await _hotelRepository.GetAllAsync(cancellationToken);

        var candidates = hotels
            .Select(hotel => new SearchCandidate(
                hotel,
                CalculateDistanceInKm(parsedPrompt.Location, hotel.Location)))
            .ToList();

        if (parsedPrompt.Budget.HasValue)
        {
            candidates = candidates
                .Where(candidate => candidate.Hotel.Price <= parsedPrompt.Budget.Value)
                .ToList();
        }

        var totalCount = candidates.Count;

        if (totalCount == 0)
        {
            return new PagedResponse<SearchHotelResponse>
            {
                Items = Array.Empty<SearchHotelResponse>(),
                Page = page,
                PageSize = pageSize,
                TotalCount = 0,
                TotalPages = 0
            };
        }

        var minPrice = candidates.Min(candidate => candidate.Hotel.Price);
        var maxPrice = candidates.Max(candidate => candidate.Hotel.Price);
        var minDistance = candidates.Min(candidate => candidate.DistanceInKm);
        var maxDistance = candidates.Max(candidate => candidate.DistanceInKm);

        var items = candidates
            .Select(candidate => new
            {
                candidate.Hotel,
                candidate.DistanceInKm,
                Score = CalculateScore(
                    candidate.Hotel.Price,
                    minPrice,
                    maxPrice,
                    candidate.DistanceInKm,
                    minDistance,
                    maxDistance)
            })
            .OrderBy(candidate => candidate.Score)
            .ThenBy(candidate => candidate.Hotel.Price)
            .ThenBy(candidate => candidate.DistanceInKm)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(candidate => new SearchHotelResponse
            {
                Name = candidate.Hotel.Name,
                Price = candidate.Hotel.Price,
                DistanceInKm = Math.Round(candidate.DistanceInKm, 2)
            })
            .ToList();

        return new PagedResponse<SearchHotelResponse>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    private static ParsedSearchPrompt ParsePrompt(string prompt)
    {
        var locationMatch = SupportedLocations
            .FirstOrDefault(location => prompt.Contains(location.Key, StringComparison.OrdinalIgnoreCase));

        if (locationMatch.Key is null)
        {
            throw new ArgumentException(
                $"Search prompt must contain one of the supported locations: {string.Join(", ", SupportedLocations.Keys)}.");
        }

        var budget = TryExtractBudget(prompt);

        return new ParsedSearchPrompt(locationMatch.Value, budget);
    }

    private static decimal? TryExtractBudget(string prompt)
    {
        var keywordMatch = BudgetWithKeywordRegex.Match(prompt);

        if (keywordMatch.Success && TryParseDecimal(keywordMatch.Groups[1].Value, out var keywordBudget))
        {
            return keywordBudget;
        }

        var euroAmountMatch = EuroAmountRegex.Match(prompt);

        if (euroAmountMatch.Success && TryParseDecimal(euroAmountMatch.Groups[1].Value, out var euroBudget))
        {
            return euroBudget;
        }

        return null;
    }

    private static bool TryParseDecimal(string value, out decimal result)
    {
        return decimal.TryParse(
            value.Replace(',', '.'),
            NumberStyles.Number,
            CultureInfo.InvariantCulture,
            out result);
    }

    private static double CalculateDistanceInKm(GeoLocation from, GeoLocation to)
    {
        const double earthRadiusInKm = 6371;

        var latitudeDifference = ToRadians(to.Latitude - from.Latitude);
        var longitudeDifference = ToRadians(to.Longitude - from.Longitude);

        var fromLatitude = ToRadians(from.Latitude);
        var toLatitude = ToRadians(to.Latitude);

        var a =
            Math.Sin(latitudeDifference / 2) * Math.Sin(latitudeDifference / 2) +
            Math.Cos(fromLatitude) * Math.Cos(toLatitude) *
            Math.Sin(longitudeDifference / 2) * Math.Sin(longitudeDifference / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return earthRadiusInKm * c;
    }

    private static double ToRadians(double degrees)
    {
        return degrees * Math.PI / 180;
    }

    private static double CalculateScore(
        decimal price,
        decimal minPrice,
        decimal maxPrice,
        double distance,
        double minDistance,
        double maxDistance)
    {
        var normalizedPrice = Normalize((double)price, (double)minPrice, (double)maxPrice);
        var normalizedDistance = Normalize(distance, minDistance, maxDistance);

        return normalizedPrice * 0.5 + normalizedDistance * 0.5;
    }

    private static double Normalize(double value, double min, double max)
    {
        if (Math.Abs(max - min) < double.Epsilon)
        {
            return 0;
        }

        return (value - min) / (max - min);
    }

    private sealed record ParsedSearchPrompt(GeoLocation Location, decimal? Budget);

    private sealed record SearchCandidate(Hotel Hotel, double DistanceInKm);
}