using Assessment.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Assessment.Infrastructure.Persistence.Configurations;

public class DonorConfiguration : IEntityTypeConfiguration<Donor>
{
    public void Configure(EntityTypeBuilder<Donor> builder)
    {
        builder.ToTable("Donors");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(d => d.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(d => d.Email)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(d => d.Email);

        builder.Property(d => d.Phone)
            .HasMaxLength(30);

        builder.Property(d => d.TotalDonatedAmount)
            .HasPrecision(18, 2);

        builder.HasMany(d => d.Donations)
            .WithOne(dn => dn.Donor)
            .HasForeignKey(dn => dn.DonorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
