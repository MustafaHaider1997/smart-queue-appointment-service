using DomainAppointment = Appointment.Domain.Entities.Appointment;
using Appointment.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Appointment.Infrastructure.Repositories
{
    public class AppointmentRepository
    {
        private readonly AppDbContext _db;

        public AppointmentRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<DomainAppointment>> GetAllAsync() =>
            await _db.Appointments.ToListAsync();

        public async Task<DomainAppointment?> GetByIdAsync(Guid id) =>
            await _db.Appointments
                .AsNoTracking() // ✅ Add this
                .FirstOrDefaultAsync(a => a.Id == id);

        public async Task AddAsync(DomainAppointment appt)
        {
            await _db.Appointments.AddAsync(appt);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(DomainAppointment appointment)
        {
            var existing = await _db.Appointments
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == appointment.Id);

            if (existing == null)
                throw new KeyNotFoundException("Appointment not found");

            _db.Appointments.Update(appointment);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var appt = await _db.Appointments.FindAsync(id);
            if (appt != null)
            {
                _db.Appointments.Remove(appt);
                await _db.SaveChangesAsync();
            }
        }
    }
}