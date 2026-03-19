using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Morourak.Domain.Entities;

namespace Morourak.Infrastructure.Persistence.Configurations
{
    public class VehicleLicenseApplicationConfiguration : IEntityTypeConfiguration<VehicleLicenseApplication>
    {
        public void Configure(EntityTypeBuilder<VehicleLicenseApplication> builder)
        {
            builder.HasKey(v => v.Id);

            builder.HasOne(v => v.Citizen)
                .WithMany()
                .HasForeignKey(v => v.CitizenRegistryId)
                .OnDelete(DeleteBehavior.Restrict);



            builder.Property(v => v.OwnershipProofPath).IsRequired(false);
            builder.Property(v => v.VehicleDataCertificatePath).IsRequired(false);
            builder.Property(v => v.IdCardPath).IsRequired(false);
            builder.Property(v => v.InsuranceCertificatePath).IsRequired(false);
            builder.Property(v => v.CustomClearancePath).IsRequired(false);
        }
    }
}