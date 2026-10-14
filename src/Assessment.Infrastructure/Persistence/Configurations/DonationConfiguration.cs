using Assessment.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Assessment.Infrastructure.Persistence.Configurations;

public class DonationConfiguration : IEntityTypeConfiguration<Donation>
{
    public void Configure(EntityTypeBuilder<Donation> builder)
    {
        builder.ToTable("Donations");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Amount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(d => d.DonationType)
            .IsRequired();

        builder.Property(d => d.Status)
            .IsRequired();

        builder.Property(d => d.Notes)
            .HasMaxLength(1000);

        builder.Property(d => d.ApprovedByUserId)
            .HasMaxLength(100);

        builder.HasIndex(d => d.DonorId);
        builder.HasIndex(d => d.Status);
        builder.HasIndex(d => d.DonationDate);
    }
}
