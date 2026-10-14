using Assessment.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Assessment.Infrastructure.Persistence.Configurations;

public class RevokedTokenConfiguration : IEntityTypeConfiguration<RevokedToken>
{
    public void Configure(EntityTypeBuilder<RevokedToken> builder)
    {
        builder.ToTable("RevokedTokens");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Token)
            .IsRequired()
            .HasMaxLength(2000);

        builder.HasIndex(r => r.Token);

        builder.Property(r => r.Reason)
            .HasMaxLength(250);
    }
}
