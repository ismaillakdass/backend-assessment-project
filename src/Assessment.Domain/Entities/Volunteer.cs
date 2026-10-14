using Assessment.Domain.Common;

namespace Assessment.Domain.Entities;

public class Volunteer : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string? Skills { get; set; }

    public ICollection<EventVolunteer> EventVolunteers { get; set; } = new List<EventVolunteer>();
}
