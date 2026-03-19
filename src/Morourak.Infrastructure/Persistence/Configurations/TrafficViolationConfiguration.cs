using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Morourak.Domain.Entities;

namespace Morourak.Infrastructure.Persistence.Configurations
{
    public class TrafficViolationConfiguration : IEntityTypeConfiguration<TrafficViolation>
    {
        public void Configure(EntityTypeBuilder<TrafficViolation> builder)
        {
            builder.HasKey(v => v.Id);

            builder.Property(v => v.ViolationNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasIndex(v => v.ViolationNumber)
                .IsUnique();

            builder.Property(v => v.LegalReference)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(v => v.Description)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(v => v.Location)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(v => v.FineAmount)
                .HasColumnType("decimal(18,2)");

            builder.Property(v => v.PaidAmount)
                .HasColumnType("decimal(18,2)");

            // Composite index for fast lookups by license
            builder.HasIndex(v => new { v.RelatedLicenseId, v.LicenseType });

            // Index for citizen queries
            builder.HasIndex(v => v.CitizenRegistryId);
        }
    }
}
