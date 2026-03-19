using Microsoft.EntityFrameworkCore;
using Morourak.Domain.Entities;

namespace Morourak.Infrastructure.Persistence
{
    /// <summary>
    /// Main database context for business and governmental data.
    /// This context is completely separated from Identity database.
    /// </summary>
    public class PersistenceDbContext : DbContext
    {
        public PersistenceDbContext(DbContextOptions<PersistenceDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Mock governmental citizen registry.
        /// Used to validate National ID and Mobile Number during registration.
        /// </summary>
        public DbSet<CitizenRegistry> CitizenRegistries { get; set; } = null!;

        public DbSet<DrivingLicense> DrivingLicenses { get; set; } = null!;
        public DbSet<VehicleLicense> VehicleLicenses { get; set; } = null!;
        public DbSet<VehicleTypeEntity> VehicleTypes { get; set; } = null!;
        public DbSet<VehicleLicenseApplication> VehicleLicenseApplications { get; set; } = null!;
        public DbSet<Appointment> ExaminationAppointments { get; set; } = null!;
        public DbSet<EmailOtp> EmailOtps { get; set; } = null!;
        public DbSet<ServiceRequest> ServiceRequests { get; set; }
        public DbSet<RenewalApplication> RenewalApplications { get; set; }
        public DbSet<VehicleViolation> VehicleViolations { get; set; }
        public DbSet<TrafficViolation> TrafficViolations { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<PaymentViolation> PaymentViolations { get; set; }

        /// <summary>
        /// جداول المحافظات ووحدات المرور — تُستخدم كبيانات مرجعية لقوائم الاختيار.
        /// </summary>
        public DbSet<Governorate> Governorates { get; set; } = null!;
        public DbSet<TrafficUnit> TrafficUnits { get; set; } = null!;
        public DbSet<Location> Locations { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(PersistenceDbContext).Assembly);

            // Ensure NationalId is unique (one citizen per National ID)
            modelBuilder.Entity<CitizenRegistry>()
                .HasIndex(c => c.NationalId)
                .IsUnique();

            // DrivingLicense -> CitizenRegistry
            modelBuilder.Entity<DrivingLicense>()
                .HasOne(d => d.Citizen)
                .WithMany()
                .HasForeignKey(d => d.CitizenRegistryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DrivingLicense>()
                .HasIndex(d => d.LicenseNumber)
                .IsUnique();

            // VehicleLicense -> CitizenRegistry
            modelBuilder.Entity<VehicleLicense>()
                .HasOne(v => v.Citizen)
                .WithMany(c => c.VehicleLicenses)
                .HasForeignKey(v => v.CitizenRegistryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<VehicleLicense>()
                .HasIndex(v => v.VehicleLicenseNumber)
                .IsUnique();

            modelBuilder.Entity<VehicleLicense>()
                .HasOne(v => v.Examination)    
                .WithOne()                     
                .HasForeignKey<VehicleLicense>(v => v.ExaminationId)
                .IsRequired(false);


            // ServiceRequest PK is configured in ServiceRequestConfiguration


            modelBuilder.Entity<DrivingLicense>(entity =>
            {
                entity.OwnsOne(e => e.DeliveryAddress, da =>
                {
                    da.Property(d => d.Governorate).HasMaxLength(100);
                    da.Property(d => d.City).HasMaxLength(100);
                    da.Property(d => d.Details).HasMaxLength(250);
                });
            });


            modelBuilder.Entity<DrivingLicense>()
                .HasMany(d => d.Applications)
                .WithOne(a => a.DrivingLicense)
                .HasForeignKey(a => a.DrivingLicenseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RenewalApplication>()
                .HasOne(r => r.Citizen)
                .WithMany()
                .HasForeignKey(r => r.CitizenRegistryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RenewalApplication>()
                .HasOne(r => r.DrivingLicense)
                .WithMany()
                .HasForeignKey(r => r.DrivingLicenseId)
                .OnDelete(DeleteBehavior.Restrict);

            // TrafficViolation -> CitizenRegistry
            modelBuilder.Entity<TrafficViolation>()
                .HasOne(v => v.Citizen)
                .WithMany()
                .HasForeignKey(v => v.CitizenRegistryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Governorate -> TrafficUnit (1-to-many)
            modelBuilder.Entity<TrafficUnit>()
                .HasOne(t => t.Governorate)
                .WithMany(g => g.TrafficUnits)
                .HasForeignKey(t => t.GovernorateId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Governorate>()
                .HasIndex(g => g.Name)
                .IsUnique();

        }
    }
}