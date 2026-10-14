using Assessment.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Assessment.Infrastructure.Persistence.Configurations;

public class VolunteerConfiguration : IEntityTypeConfiguration<Volunteer>
{
    public void Configure(EntityTypeBuilder<Volunteer> builder)
    {
        builder.ToTable("Volunteers");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(v => v.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(v => v.Email)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(v => v.Phone)
            .HasMaxLength(30);

        builder.Property(v => v.City)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(v => v.Skills)
            .HasMaxLength(500);

        builder.HasIndex(v => v.City);
        builder.HasIndex(v => v.IsActive);
    }
}
