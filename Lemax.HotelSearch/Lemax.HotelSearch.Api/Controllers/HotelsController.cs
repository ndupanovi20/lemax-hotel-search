using Lemax.HotelSearch.Application.Dtos;
using Lemax.HotelSearch.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lemax.HotelSearch.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class HotelsController : ControllerBase
{
    private readonly IHotelService _hotelService;

    public HotelsController(IHotelService hotelService)
    {
        _hotelService = hotelService ?? throw new ArgumentNullException(nameof(hotelService));
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<HotelResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var hotels = await _hotelService.GetAllAsync(cancellationToken);

        return Ok(hotels);
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