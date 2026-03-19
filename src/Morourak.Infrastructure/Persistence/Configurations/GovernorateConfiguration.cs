using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Morourak.Domain.Entities;

namespace Morourak.Infrastructure.Persistence.Configurations
{
    public class GovernorateConfiguration : IEntityTypeConfiguration<Governorate>
    {
        public void Configure(EntityTypeBuilder<Governorate> builder)
        {
            builder.Property(g => g.Name).IsRequired().HasMaxLength(250);

            builder.HasIndex(g => g.Name).IsUnique();

            builder.HasIndex(g => g.Name).IsUnique();
        }
    }
}
