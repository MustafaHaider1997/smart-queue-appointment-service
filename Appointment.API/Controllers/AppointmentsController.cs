using DomainAppointment = Appointment.Domain.Entities.Appointment;
using Appointment.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Appointment.API.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/appointments")]
public class AppointmentsController : ControllerBase
{
    private readonly AppointmentRepository _repo;

    public AppointmentsController(AppointmentRepository repo)
    {
        _repo = repo;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var appointments = await _repo.GetAllAsync();
        return Ok(appointments);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var appt = await _repo.GetByIdAsync(id);
        return appt == null ? NotFound() : Ok(appt);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] DomainAppointment appt)
    {
        appt.Id = Guid.NewGuid();
        await _repo.AddAsync(appt);
        return CreatedAtAction(nameof(GetById), new { id = appt.Id }, appt);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] DomainAppointment appt)
    {
        var existing = await _repo.GetByIdAsync(id);
        if (existing == null) return NotFound();

        appt.Id = id;
        await _repo.UpdateAsync(appt);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var existing = await _repo.GetByIdAsync(id);
        if (existing == null) return NotFound();

        await _repo.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet("test-jwt")]
    public IActionResult TestJwt()
    {
        var sub = User.FindFirst("sub")?.Value;
        var email = User.FindFirst("email")?.Value;
        return Ok(new { sub, email });
    }

}