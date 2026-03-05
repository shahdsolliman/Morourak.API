using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Morourak.Domain.Entities;

namespace Morourak.Infrastructure.Persistence.Configurations
{
    public class LocationConfiguration : IEntityTypeConfiguration<Location>
    {
        public void Configure(EntityTypeBuilder<Location> builder)
        {
            builder.Property(l => l.Name).IsRequired().HasMaxLength(250);

            builder.HasOne(l => l.TrafficUnit)
                .WithMany(u => u.Locations)
                .HasForeignKey(l => l.TrafficUnitId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
