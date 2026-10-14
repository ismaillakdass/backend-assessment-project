using Assessment.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Assessment.Infrastructure.Persistence.Configurations;

public class EventVolunteerConfiguration : IEntityTypeConfiguration<EventVolunteer>
{
    public void Configure(EntityTypeBuilder<EventVolunteer> builder)
    {
        builder.ToTable("EventVolunteers");

        builder.HasKey(ev => new { ev.EventId, ev.VolunteerId });

        builder.HasOne(ev => ev.Event)
            .WithMany(e => e.EventVolunteers)
            .HasForeignKey(ev => ev.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ev => ev.Volunteer)
            .WithMany(v => v.EventVolunteers)
            .HasForeignKey(ev => ev.VolunteerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
