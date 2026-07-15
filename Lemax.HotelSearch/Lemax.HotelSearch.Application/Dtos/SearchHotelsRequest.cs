namespace Lemax.HotelSearch.Application.Dtos;

public sealed class SearchHotelsRequest
{
    public string Prompt { get; set; } = string.Empty;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}