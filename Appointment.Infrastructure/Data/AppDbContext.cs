using Microsoft.EntityFrameworkCore;
using DomainAppointment = Appointment.Domain.Entities.Appointment;

namespace Appointment.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<DomainAppointment> Appointments => Set<DomainAppointment>();
    }
}