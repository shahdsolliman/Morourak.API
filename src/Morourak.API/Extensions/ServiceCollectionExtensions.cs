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
using Morourak.Application.Configurations;

namespace Morourak.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(
            this IServiceCollection services,
            IConfiguration configuration,
            IWebHostEnvironment env)
        {
            services.AddDatabase(configuration, env);
            services.AddIdentityServices();
            services.AddApplicationServicesCore(configuration);
            services.AddLicenseServices(configuration);
            services.AddCaching(configuration);
            services.AddExternalIntegrations(configuration);
            // services.AddJwtAuthentication(configuration); // Removed: Already called in Program.cs

            return services;
        }

        public static IServiceCollection AddDatabase(
            this IServiceCollection services,
            IConfiguration configuration,
            IWebHostEnvironment env)
        {
            var identityConn = ResolveConnectionString(configuration, env, "IdentityConnection");
            var persistenceConn = ResolveConnectionString(configuration, env, "PersistenceConnection");

            if (!string.IsNullOrEmpty(identityConn))
            {
                services.AddDbContext<IdentityDbContext>((sp, options) =>
                {
                    options.UseSqlServer(identityConn, sql => 
                        sql.EnableRetryOnFailure()
                           .MigrationsHistoryTable("__IdentityMigrationsHistory"));
                });
            }

            if (!string.IsNullOrEmpty(persistenceConn))
            {
                services.AddDbContext<PersistenceDbContext>((sp, options) =>
                {
                    options.UseSqlServer(persistenceConn, sql => 
                        sql.EnableRetryOnFailure()
                           .MigrationsHistoryTable("__PersistenceMigrationsHistory"));
                    
                    if (env.IsDevelopment())
                    {
                        options.EnableSensitiveDataLogging();
                    }
                });
            }

            return services;
        }

        public static IServiceCollection AddIdentityServices(this IServiceCollection services)
        {
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 8;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<IdentityDbContext>()
            .AddDefaultTokenProviders();

            return services;
        }

        public static IServiceCollection AddApplicationServicesCore(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<ICitizenRegistryService, CitizenRegistryService>();
            services.AddScoped<ICitizenService, CitizenService>();
            services.AddScoped<IAppointmentService, AppointmentService>();
            services.AddScoped<IAppointmentQueryService, AppointmentQueryService>();
            services.AddScoped<IAppointmentDomainService, AppointmentDomainService>();
            services.AddScoped<IRequestDomainService, RequestDomainService>();
            services.AddScoped<IOtpService, OtpService>();
            services.AddScoped<IMailService, MailService>();
            services.AddScoped<IServiceRequestService, ServiceRequestService>();
            services.AddScoped<IRequestNumberGenerator, RequestNumberGenerator>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IApplicationValidationService, ApplicationValidationService>();
            services.AddScoped<ITrafficViolationService, TrafficViolationService>();
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IGovernorateService, GovernorateService>();
            services.AddScoped<IIdentityService, IdentityService>();
            services.AddScoped<IAdminSeedDataService, AdminSeedDataService>();
            services.AddScoped<IVehicleLicenseService, VehicleLicenseService>();
            services.AddScoped<IAdminUserService, AdminUserService>();

            services.AddHttpContextAccessor();
            
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(InvalidationBehavior<,>));

            return services;
        }

        public static IServiceCollection AddLicenseServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<ILicenseValidationService, LicenseValidationService>();
            services.AddScoped<IDrivingLicenseIssuanceService, DrivingLicenseIssuanceService>();
            services.AddScoped<IDrivingLicenseRenewalService, DrivingLicenseRenewalService>();
            services.AddScoped<IDrivingLicenseReplacementService, DrivingLicenseReplacementService>();
            services.AddScoped<IDrivingLicenseQueryService, DrivingLicenseQueryService>();
            services.AddScoped<IDrivingLicenseResultService, DrivingLicenseResultService>();
            
            // Facade service
            services.AddScoped<IDrivingLicenseService, DrivingLicenseService>();

            // Config for durations
            services.Configure<LicenseSettings>(configuration.GetSection(LicenseSettings.SectionName));

            return services;
        }

        public static IServiceCollection AddCaching(this IServiceCollection services, IConfiguration configuration)
        {
            var redisSection = configuration.GetSection("RedisSettings");
            services.Configure<RedisSettings>(redisSection);

            var redisSettings = redisSection.Get<RedisSettings>();
            if (redisSettings != null && !string.IsNullOrWhiteSpace(redisSettings.ConnectionString))
            {
                // Register IConnectionMultiplexer with a SAFE factory that won't throw during resolution
                services.AddSingleton<IConnectionMultiplexer>(sp =>
                {
                    try
                    {
                        var options = ConfigurationOptions.Parse(redisSettings.ConnectionString);
                        options.AbortOnConnectFail = false; // Important: Prevents crashing if server down on start
                        options.ConnectRetry = 1;
                        options.ConnectTimeout = 2000;
                        return ConnectionMultiplexer.Connect(options);
                    }
                    catch (Exception ex)
                    {
                        var logger = sp.GetRequiredService<ILogger<RedisSettings>>();
                        logger.LogError(ex, "CRITICAL: Redis Connection Failed. Using disconnected multiplexer.");
                        // Return a disconnected multiplexer instead of throwing
                        return null!; // Even returning null is better than throwing, but let's be safer
                    }
                });

                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = redisSettings.ConnectionString;
                    options.InstanceName = "Morourak_";
                });

                services.AddScoped<ICacheService, RedisCacheService>();
            }
            else
            {
                services.AddDistributedMemoryCache();
                services.AddScoped<ICacheService, NoOpCacheService>();
            }

            return services;
        }

        public static IServiceCollection AddExternalIntegrations(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
            services.Configure<PayMobSettings>(configuration.GetSection("PayMob"));
            services.Configure<PaymentSettings>(configuration.GetSection("PaymentSettings"));

            services.AddHttpClient<IPayMobService, PayMobService>();
            
            return services;
        }

        private static string ResolveConnectionString(IConfiguration configuration, IWebHostEnvironment environment, string name)
        {
            var value = configuration.GetConnectionString(name);
            if (environment.IsDevelopment() && string.IsNullOrEmpty(value))
            {
                throw new InvalidOperationException($"Connection string {name} is missing.");
            }
            return value ?? string.Empty;
        }
    }
}
