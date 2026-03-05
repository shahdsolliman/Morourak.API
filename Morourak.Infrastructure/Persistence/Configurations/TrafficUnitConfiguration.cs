using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Morourak.Domain.Entities;

namespace Morourak.Infrastructure.Persistence.Configurations
{
    public class TrafficUnitConfiguration : IEntityTypeConfiguration<TrafficUnit>
    {
        public void Configure(EntityTypeBuilder<TrafficUnit> builder)
        {
            builder.Property(t => t.Name).IsRequired().HasMaxLength(250);

            builder.HasOne(t => t.Governorate)
                .WithMany(g => g.TrafficUnits)
                .HasForeignKey(t => t.GovernorateId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
