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

            // Seeding Data
            var seedDate = DateTime.SpecifyKind(new DateTime(2024, 1, 1), DateTimeKind.Utc);
            builder.HasData(
                new Location { Id = 1, Name = "صالة الانتظار الرئيسية", TrafficUnitId = 1, CreatedAt = seedDate },
                new Location { Id = 2, Name = "مكتب تراخيص المركبات", TrafficUnitId = 1, CreatedAt = seedDate },
                new Location { Id = 3, Name = "مكتب التحريات", TrafficUnitId = 1, CreatedAt = seedDate },
                new Location { Id = 4, Name = "خزينة السداد", TrafficUnitId = 2, CreatedAt = seedDate },
                new Location { Id = 5, Name = "صالة الفحص الفني", TrafficUnitId = 2, CreatedAt = seedDate },
                new Location { Id = 6, Name = "مكتب استلام الرخص", TrafficUnitId = 3, CreatedAt = seedDate },
                new Location { Id = 7, Name = "نيابة المرور", TrafficUnitId = 3, CreatedAt = seedDate }
            );
        }
    }
}
