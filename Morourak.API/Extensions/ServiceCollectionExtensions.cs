using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Morourak.Application.Interfaces;
using Morourak.Application.Interfaces.Services;
using Morourak.Application.Services;
using Morourak.Application.Services.Licenses;
using Morourak.Domain.Enums.Request;
using Morourak.Infrastructure.Identity;
using Morourak.Infrastructure.Persistence;
using Morourak.Infrastructure.Settings;
using Morourak.Infrastructure.UnitOfWork;
using Microsoft.Extensions.Logging;
using Morourak.Infrastructure.Services;

namespace Morourak.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        private static string ResolveConnectionString(IConfiguration configuration, string name)
        {
            var value = configuration.GetConnectionString(name);

            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidOperationException($"Connection string '{name}' is missing or empty.");

            // appsettings.json uses placeholders like "ENV_IDENTITY_CONNECTION_STRING".
            // If so, read the real connection string from environment variables.
            if (value.StartsWith("ENV_", StringComparison.Ordinal))
            {
                var envValue = Environment.GetEnvironmentVariable(value);
                if (string.IsNullOrWhiteSpace(envValue))
                    throw new InvalidOperationException(
                        $"Connection string '{name}' is set to placeholder '{value}'. Set the environment variable '{value}' to a valid SQL Server connection string.");

                return envValue;
            }

            return value;
        }

        public static IServiceCollection AddApplicationServices(
    this IServiceCollection services,
    IConfiguration configuration,
    IWebHostEnvironment env)
        {
            // Identity Database
            services.AddDbContext<IdentityDbContext>(options =>
                options.UseSqlServer(ResolveConnectionString(configuration, "IdentityConnection"))
            );

            // Persistence Database
            services.AddDbContext<PersistenceDbContext>(options =>
            {
                options.UseSqlServer(ResolveConnectionString(configuration, "PersistenceConnection"));

                // ── FIX: Sensitive data logging ONLY in Development ──────────
                // In Production, EF Core logs must NEVER contain SQL parameter
                // values (National IDs, amounts, personal data) — PDPL violation.
                // ─────────────────────────────────────────────────────────────
                if (env.IsDevelopment())
                {
                    options.EnableSensitiveDataLogging();
                    options.LogTo(Console.WriteLine, LogLevel.Information);
                }
            });

            // Identity Configuration
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;

                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<IdentityDbContext>()
            .AddDefaultTokenProviders();

            // Authentication
            services.AddAuthentication();

            // Application Services
            services.AddScoped<ICitizenRegistryService, CitizenRegistryService>();
            services.AddScoped<IAppointmentService, AppointmentService>();
            services.AddScoped<IOtpService, OtpService>();
            services.AddScoped<IMailService, MailService>();  
            services.AddScoped<IVehicleLicenseService, VehicleLicenseService>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IServiceRequestService, ServiceRequestService>();
            services.AddScoped<IRequestNumberGenerator, RequestNumberGenerator>();
            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IDrivingLicenseService, DrivingLicenseService>();
            services.AddScoped<IApplicationValidationService, ApplicationValidationService>();
            services.AddScoped<ITrafficViolationService, TrafficViolationService>();
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IAdminUserService, Morourak.Infrastructure.Services.AdminUserService>();
            services.AddScoped<IGovernorateService, GovernorateService>();
            services.AddScoped<IArabicDataService, ArabicDataService>();
            // EmailSettings
            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));

            // PayMob Settings
            services.Configure<PayMobSettings>(configuration.GetSection("PayMob"));
            services.AddHttpClient<IPayMobService, Morourak.Infrastructure.Services.PayMobService>();

            return services;
        }
    }
}
