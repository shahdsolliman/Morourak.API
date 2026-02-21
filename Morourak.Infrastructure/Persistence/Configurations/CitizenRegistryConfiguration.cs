using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Morourak.Domain.Entities;

namespace Morourak.Infrastructure.Persistence.Configurations
{
    public class CitizenRegistryConfiguration : IEntityTypeConfiguration<CitizenRegistry>
    {
        public void Configure(EntityTypeBuilder<CitizenRegistry> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.NationalId)
                .IsRequired()
                .HasMaxLength(14);

            builder.Property(c => c.NameAr)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.FatherFirstNameAr)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(c => c.MobileNumber)
                .IsRequired()
                .HasMaxLength(15);

            builder.HasIndex(c => c.NationalId).IsUnique();
        }
    }
}