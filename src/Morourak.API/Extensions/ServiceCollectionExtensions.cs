using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Morourak.Application.Interfaces;
using Morourak.Application.Interfaces.DomainServices;
using Morourak.Application.Interfaces.Services;
using Morourak.Application.DomainServices.Appointment;
using Morourak.Application.Services;
using Morourak.Application.Services.Licenses;
using Morourak.Domain.Enums.Request;
using Morourak.Infrastructure.Identity;
using Morourak.Infrastructure.Persistence;
using Morourak.Infrastructure.Settings;
using Morourak.Infrastructure.UnitOfWork;
using Morourak.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Morourak.Application.Common.Behaviours;
using MediatR;
using StackExchange.Redis;

namespace Morourak.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        private static string ResolveConnectionString(
            IConfiguration configuration,
            IWebHostEnvironment environment,
            string name,
            ILogger? logger = null)
        {
            var value = configuration.GetConnectionString(name);
            string? source = "appsettings.json";

            // If configuration has a value that starts with ENV_, treat the rest as the env var name.
            string? envVarName = null;
            if (!string.IsNullOrWhiteSpace(value) &&
                value.StartsWith("ENV_", StringComparison.OrdinalIgnoreCase))
            {
                envVarName = value.Substring("ENV_".Length);
                source = $"Environment Variable ({envVarName})";
            }

            // Optional naming convention fallback (e.g. PERSISTENCE_CONNECTION_STRING)
            if (string.IsNullOrWhiteSpace(envVarName))
            {
                envVarName = name.ToUpperInvariant() switch
                {
                    "IDENTITYCONNECTION" => "IDENTITY_CONNECTION_STRING",
                    "PERSISTENCECONNECTION" => "PERSISTENCE_CONNECTION_STRING",
                    _ => null
                };
            }

            if (!string.IsNullOrWhiteSpace(envVarName))
            {
                var fromEnv = Environment.GetEnvironmentVariable(envVarName);
                if (!string.IsNullOrWhiteSpace(fromEnv))
                {
                    source = $"Environment Variable ({envVarName})";
                    value = fromEnv;
                }
            }

            // In Development, allow falling back to appsettings if it's not an ENV_ placeholder.
            if (environment.IsDevelopment())
            {
                if (!string.IsNullOrWhiteSpace(value) &&
                    !value.StartsWith("ENV_", StringComparison.OrdinalIgnoreCase))
                {
                    // Log the resolution details (masking password)
                    var logValue = MaskConnectionString(value);
                    logger?.LogInformation("Resolved connection string '{Name}' from {Source}: {Value}", name, source, logValue);
                    return value!;
                }

                throw new InvalidOperationException(
                    $"Connection string '{name}' is not configured. " +
                    $"In Development you must set a real value for '{name}' in 'appsettings.Development.json'.");
            }

            // In non‑development environments, we require environment variables.
            if (string.IsNullOrWhiteSpace(value) || value.StartsWith("ENV_", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"Connection string '{name}' is not configured for environment '{environment.EnvironmentName}'. " +
                    $"Expected environment variable '{envVarName ?? "[ENV_VAR_NAME]"}'.");
            }

            return value;
        }

        private static string MaskConnectionString(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString)) return string.Empty;
            
            var parts = connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i].Contains("Password", StringComparison.OrdinalIgnoreCase) || 
                    parts[i].Contains("Pwd", StringComparison.OrdinalIgnoreCase))
                {
                    parts[i] = "Password=********";
                }
            }
            return string.Join(";", parts);
        }

        public static IServiceCollection AddApplicationServices(
            this IServiceCollection services,
            IConfiguration configuration,
            IWebHostEnvironment env)
        {
            // Distributed cache (uses in-memory implementation by default; can be swapped for Redis).
            services.AddDistributedMemoryCache();
            // Pre-validate connection strings to maintain fail-fast behavior
            ResolveConnectionString(configuration, env, "IdentityConnection");
            ResolveConnectionString(configuration, env, "PersistenceConnection");

            // Identity Database
            services.AddDbContext<IdentityDbContext>((sp, options) =>
            {
                var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseConfiguration");
                options.UseSqlServer(ResolveConnectionString(configuration, env, "IdentityConnection", logger));
            });

            // Persistence Database
            services.AddDbContext<PersistenceDbContext>((sp, options) =>
            {
                var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseConfiguration");
                options.UseSqlServer(ResolveConnectionString(configuration, env, "PersistenceConnection", logger));

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
            services.AddScoped<ICitizenService, CitizenService>();
            services.AddScoped<IAppointmentService, AppointmentService>();
            services.AddScoped<IAppointmentQueryService, AppointmentQueryService>();
            // Appointment CQRS domain services
            services.AddScoped<IAppointmentDomainService, AppointmentDomainService>();
            services.AddScoped<IRequestDomainService, RequestDomainService>();
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
            services.AddScoped<IIdentityService, IdentityService>();
            
            // Settings & Configurations
            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
            services.Configure<PayMobSettings>(configuration.GetSection("PayMob"));
            services.AddHttpClient<IPayMobService, Morourak.Infrastructure.Services.PayMobService>();

            // Redis Caching
            var redisSettings = configuration.GetSection("RedisSettings").Get<RedisSettings>() ?? new RedisSettings();
            services.Configure<RedisSettings>(configuration.GetSection("RedisSettings"));
            
            services.AddSingleton<IConnectionMultiplexer>(sp => 
                ConnectionMultiplexer.Connect(redisSettings.ConnectionString));

            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisSettings.ConnectionString;
                options.InstanceName = "Morourak_";
            });

            services.AddScoped<ICacheService, RedisCacheService>();

            // MediatR Pipeline Behaviors for Caching
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(InvalidationBehavior<,>));

            return services;
        }
    }
}
