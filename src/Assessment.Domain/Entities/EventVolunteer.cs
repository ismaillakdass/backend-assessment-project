namespace Assessment.Domain.Entities;

public class EventVolunteer
{
    public int EventId { get; set; }
    public Event? Event { get; set; }

    public int VolunteerId { get; set; }
    public Volunteer? Volunteer { get; set; }

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
}
