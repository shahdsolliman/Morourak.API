using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Morourak.Domain.Entities;

namespace Morourak.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.TransactionId)
            .HasMaxLength(100);

        builder.Property(p => p.MerchantOrderId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Amount)
            .HasColumnType("decimal(18,2)");

        builder.Property(p => p.Currency)
            .HasMaxLength(3);

        builder.Property(p => p.CitizenNationalId)
            .IsRequired()
            .HasMaxLength(14);

        builder.HasOne(p => p.ServiceRequest)
            .WithMany()
            .HasForeignKey(p => p.ServiceRequestNumber)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => p.TransactionId)
               .IsUnique()
               .HasFilter("[TransactionId] IS NOT NULL");
        builder.HasIndex(p => p.MerchantOrderId).IsUnique();
    }
}

public class PaymentViolationConfiguration : IEntityTypeConfiguration<PaymentViolation>
{
    public void Configure(EntityTypeBuilder<PaymentViolation> builder)
    {
        builder.HasKey(pv => new { pv.PaymentId, pv.TrafficViolationId });

        builder.Property(pv => pv.AmountPaid)
            .HasColumnType("decimal(18,2)");

        builder.HasOne(pv => pv.Payment)
            .WithMany(p => p.PaymentViolations)
            .HasForeignKey(pv => pv.PaymentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pv => pv.TrafficViolation)
            .WithMany()
            .HasForeignKey(pv => pv.TrafficViolationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
