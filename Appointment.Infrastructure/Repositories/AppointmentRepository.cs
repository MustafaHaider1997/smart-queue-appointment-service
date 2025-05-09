using DomainAppointment = Appointment.Domain.Entities.Appointment;
using Appointment.Infrastructure.Data;
using Appointment.Infrastructure.Redis;
using Microsoft.EntityFrameworkCore;

namespace Appointment.Infrastructure.Repositories
{
    public class AppointmentRepository
    {
        private readonly AppDbContext _db;
        private readonly RedisPublisher _redis;

        public AppointmentRepository(AppDbContext db, RedisPublisher redis)
        {
            _db = db;
            _redis = redis;
        }

        public async Task<List<DomainAppointment>> GetAllAsync() =>
            await _db.Appointments.ToListAsync();

        public async Task<DomainAppointment?> GetByIdAsync(Guid id) =>
            await _db.Appointments
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id);

        public async Task<List<DomainAppointment>> GetByUserIdAsync(string userId)
        {
            return await _db.Appointments
                .Where(a => a.UserId == userId)
                .ToListAsync();
        }

        public async Task AddAsync(DomainAppointment appt)
        {
            await _db.Appointments.AddAsync(appt);
            await _db.SaveChangesAsync();

            var message = $"New appointment created with ID: {appt.Id}, Time: {appt.ScheduledTime}";
            await _redis.PublishAsync("queue_updates", message);
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

            var message = $"Appointment updated with ID: {appointment.Id}, New Time: {appointment.ScheduledTime}";
            await _redis.PublishAsync("queue_updates", message);
        }

        public async Task DeleteAsync(Guid id)
        {
            var appt = await _db.Appointments.FindAsync(id);
            if (appt != null)
            {
                _db.Appointments.Remove(appt);
                await _db.SaveChangesAsync();

                var message = $"Appointment cancelled with ID: {appt.Id}, Time: {appt.ScheduledTime}";
                await _redis.PublishAsync("queue_updates", message);
            }
        }
    }
}