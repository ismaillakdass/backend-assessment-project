namespace Assessment.Application.DTOs.Events;

public class EventDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public int Capacity { get; set; }
    public int RegisteredVolunteersCount { get; set; }
    public bool IsFull => RegisteredVolunteersCount >= Capacity;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateEventDto
{
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public int Capacity { get; set; }
}

public class AssignVolunteerDto
{
    public int VolunteerId { get; set; }
}
