using Lemax.HotelSearch.Application.Dtos;

namespace Lemax.HotelSearch.Application.Services;

public interface IHotelSearchService
{
    Task<PagedResponse<SearchHotelResponse>> SearchAsync(
        SearchHotelsRequest request,
        CancellationToken cancellationToken = default);
}