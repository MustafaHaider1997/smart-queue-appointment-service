namespace Appointment.Domain.Entities;

public class Appointment
{
    public Guid Id { get; set; }  // Unique appointment ID
    public string UserId { get; set; } = string.Empty;  // FK to user (from User Service)
    public DateTime ScheduledTime { get; set; }  // Slot selected
    public string Status { get; set; } = "Scheduled";  // Scheduled, Cancelled, Rescheduled
}