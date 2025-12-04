using Microsoft.AspNetCore.Mvc;
using TravelCheck.Application.Services;
using TravelCheck.Application.Dtos;

namespace TravelCheck.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TripsController : ControllerBase
{
    private readonly TripService _service;

    public TripsController(TripService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateTripDto dto)
    {
        var id = await _service.CreateAsync(dto);
        return Ok(id);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var trips = await _service.GetAllAsync();
        return Ok(trips);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var trip = await _service.GetByIdAsync(id);
        if (trip == null)
            return NotFound();

        return Ok(trip);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, UpdateTripDto dto)
    {
        var trip = await _service.UpdateAsync(id, dto);
        return Ok(trip);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var trip = await _service.DeleteAsync(id);
        return Ok(trip);
    }
}
