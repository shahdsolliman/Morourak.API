using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Morourak.Domain.Entities;

namespace Morourak.Infrastructure.Persistence.Configurations
{
    public class DrivingLicenseConfiguration : IEntityTypeConfiguration<DrivingLicense>
    {
        public void Configure(EntityTypeBuilder<DrivingLicense> builder)
        {
            builder.HasKey(d => d.Id);

            builder.Property(d => d.Id);

            builder.Property(d => d.LicenseNumber)
                .IsRequired()
                .HasMaxLength(20);

            builder.HasIndex(d => d.LicenseNumber).IsUnique();

            builder.HasOne(d => d.Citizen)
                .WithMany()
                .HasForeignKey(d => d.CitizenRegistryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(x => x.IssueDate).HasConversion(
                v => v.ToDateTime(TimeOnly.MinValue),
                v => DateOnly.FromDateTime(v));

            builder.Property(x => x.ExpiryDate).HasConversion(
                       v => v.ToDateTime(TimeOnly.MinValue),
                       v => DateOnly.FromDateTime(v));
        }
    }
}