using Assessment.Domain.Common;

namespace Assessment.Domain.Entities;

public class Event : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public int Capacity { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<EventVolunteer> EventVolunteers { get; set; } = new List<EventVolunteer>();
}
