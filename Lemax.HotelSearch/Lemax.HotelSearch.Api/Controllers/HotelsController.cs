using Lemax.HotelSearch.Application.Dtos;
using Lemax.HotelSearch.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lemax.HotelSearch.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class HotelsController : ControllerBase
{
    private readonly IHotelService _hotelService;
    private readonly IHotelSearchService _hotelSearchService;

    public HotelsController(IHotelService hotelService, IHotelSearchService hotelSearchService)
    {
        _hotelService = hotelService ?? throw new ArgumentNullException(nameof(hotelService));

        _hotelSearchService = hotelSearchService ?? throw new ArgumentNullException(nameof(hotelSearchService));
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<HotelResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var hotels = await _hotelService.GetAllAsync(cancellationToken);

        return Ok(hotels);
    }

    [HttpGet("search")]
    public async Task<ActionResult<PagedResponse<SearchHotelResponse>>> Search(
        [FromQuery] string prompt,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new SearchHotelsRequest
            {
                Prompt = prompt,
                Page = page,
                PageSize = pageSize
            };

            var result = await _hotelSearchService.SearchAsync(request, cancellationToken);

            return Ok(result);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<HotelResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var hotel = await _hotelService.GetByIdAsync(id, cancellationToken);

        if (hotel is null)
        {
            return NotFound();
        }

        return Ok(hotel);
    }

    [HttpPost]
    public async Task<ActionResult<HotelResponse>> Create(
        [FromBody] CreateHotelRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var hotel = await _hotelService.CreateAsync(request, cancellationToken);

            return CreatedAtAction(
                nameof(GetById),
                new { id = hotel.Id },
                hotel);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateHotelRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var updated = await _hotelService.UpdateAsync(id, request, cancellationToken);

            if (!updated)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _hotelService.DeleteAsync(id, cancellationToken);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}