using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Morourak.Domain.Entities;

namespace Morourak.Infrastructure.Persistence.Configurations
{
    public class RenewalApplicationConfiguration : IEntityTypeConfiguration<RenewalApplication>
    {
        public void Configure(EntityTypeBuilder<RenewalApplication> builder)
        {
            builder.HasOne(r => r.Citizen)
                .WithMany()
                .HasForeignKey(r => r.CitizenRegistryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.DrivingLicense)
                .WithMany()
                .HasForeignKey(r => r.DrivingLicenseId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
