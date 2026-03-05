using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Morourak.Domain.Entities;

public class ExaminationAppointmentConfiguration
    : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
               .ValueGeneratedOnAdd();

        builder.Property(e => e.Type)
               .IsRequired();

        builder.Property(e => e.Date)
               .IsRequired();

        builder.Property(e => e.StartTime)
               .IsRequired();

        builder.Property(e => e.EndTime)
               .IsRequired();

        builder.Property(e => e.Status)
               .IsRequired();

        builder.Property(e => e.CitizenNationalId)
               .IsRequired()
               .HasMaxLength(14);


        // ── Location FK relations (added in refactor) ──────────────────────
        builder.Property(e => e.GovernorateId)
               .IsRequired();

        builder.Property(e => e.TrafficUnitId)
               .IsRequired();

        builder.HasOne(e => e.Governorate)
               .WithMany()
               .HasForeignKey(e => e.GovernorateId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.TrafficUnit)
               .WithMany()
               .HasForeignKey(e => e.TrafficUnitId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}