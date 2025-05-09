using DomainAppointment = Appointment.Domain.Entities.Appointment;
using Appointment.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims; // ✅ Required for ClaimTypes

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
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null)
            return Unauthorized();

        var appointments = await _repo.GetByUserIdAsync(userIdClaim);
        return Ok(appointments);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        Console.WriteLine($"🔐 Decoded userId from token: {userIdClaim}");

        if (userIdClaim == null)
        {
            Console.WriteLine("❌ Token does not contain 'sub' claim.");
            return Unauthorized();
        }

        var appt = await _repo.GetByIdAsync(id);

        if (appt == null)
        {
            Console.WriteLine($"❌ No appointment found with ID: {id}");
            return NotFound();
        }

        Console.WriteLine($"✅ Fetched appointment. userId in DB: {appt.UserId}");

        if (appt.UserId != userIdClaim)
        {
            Console.WriteLine("❌ Logged-in user does not own this appointment.");
            return NotFound();
        }

        return Ok(appt);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] DomainAppointment appt)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null)
            return Unauthorized();

        appt.Id = Guid.NewGuid();
        appt.UserId = userIdClaim;
        await _repo.AddAsync(appt);
        return CreatedAtAction(nameof(GetById), new { id = appt.Id }, appt);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] DomainAppointment appt)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null)
            return Unauthorized();

        var existing = await _repo.GetByIdAsync(id);
        if (existing == null || existing.UserId != userIdClaim)
            return NotFound();

        appt.Id = id;
        appt.UserId = userIdClaim; // Ensure ownership is retained
        await _repo.UpdateAsync(appt);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null)
            return Unauthorized();

        var existing = await _repo.GetByIdAsync(id);
        if (existing == null || existing.UserId != userIdClaim)
            return NotFound();

        await _repo.DeleteAsync(id);
        return NoContent();
    }

}