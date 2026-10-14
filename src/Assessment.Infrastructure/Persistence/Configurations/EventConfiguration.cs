using Assessment.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Assessment.Infrastructure.Persistence.Configurations;

public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.ToTable("Events");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.City)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Capacity)
            .IsRequired();

        builder.HasIndex(e => e.City);
        builder.HasIndex(e => e.EventDate);
    }
}
