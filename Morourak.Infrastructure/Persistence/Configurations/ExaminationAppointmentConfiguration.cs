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
    }
}