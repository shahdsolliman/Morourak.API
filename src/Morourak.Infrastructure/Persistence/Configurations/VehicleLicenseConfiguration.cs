using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Morourak.Domain.Entities;

namespace Morourak.Infrastructure.Persistence.Configurations
{
    public class VehicleLicenseConfiguration : IEntityTypeConfiguration<VehicleLicense>
    {
        public void Configure(EntityTypeBuilder<VehicleLicense> builder)
        {
            builder.HasKey(v => v.Id);

            builder.Property(v => v.VehicleLicenseNumber)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(v => v.PlateNumber)
                .IsRequired()
                .HasMaxLength(20);

            builder.HasIndex(v => v.VehicleLicenseNumber).IsUnique();

            builder.HasOne(v => v.Citizen)
                .WithMany(c => c.VehicleLicenses)
                .HasForeignKey(v => v.CitizenRegistryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(v => v.Examination)
                .WithOne()
                .HasForeignKey<VehicleLicense>(v => v.ExaminationId)
                .IsRequired(false);

            builder.OwnsOne(x => x.DeliveryAddress);
        }
    }
}