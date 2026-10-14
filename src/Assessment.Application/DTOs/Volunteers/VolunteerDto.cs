namespace Assessment.Application.DTOs.Volunteers;

public class VolunteerDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string? Skills { get; set; }
    public DateTime CreatedAt { get; set; }
    public int AssignedEventsCount { get; set; }
}

public class CreateVolunteerDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? Skills { get; set; }
}

public class UpdateVolunteerDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string? Skills { get; set; }
}
