using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Morourak.API.Extensions;
using Morourak.API.Extensions.JsonConverters;
using Morourak.API.Formatting;
using Morourak.API.Middleware;
using Morourak.Application.CQRS.Appointment.Commands.CreateAppointment;
using Morourak.Application.CQRS.Behaviors;
using Morourak.Application.Mapping.Appointment;
using Morourak.Application.Exceptions;
using FluentValidation;
using AutoMapper;
using Serilog;
using System.Threading.RateLimiting;

namespace Morourak.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ========== Serilog Logging ==========
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            builder.Host.UseSerilog();

            // ===============================
            // Services
            // ===============================
            builder.Services
                .AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new ArabicEnumConverter());
                    options.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter());
                    options.JsonSerializerOptions.Converters.Add(new TimeOnlyJsonConverter());
                })
                .ConfigureApiBehaviorOptions(options =>
                {
                    options.InvalidModelStateResponseFactory = context =>
                    {
                        var details = context.ModelState
                            .Where(e => e.Value.Errors.Count > 0)
                            .SelectMany(x => x.Value.Errors.Select(error => new ErrorDetail
                            {
                                Field = x.Key,
                                Error = error.ErrorMessage
                            })).ToList();

                        return new BadRequestObjectResult(new
                        {
                            isSuccess = false,
                            message = "بيانات غير صالحة.",
                            errorCode = "VALIDATION_ERROR",
                            details
                        });
                    };
                });

            builder.Services.AddMemoryCache();
            builder.Services.AddApplicationServices(builder.Configuration, builder.Environment);

            // ===============================
            // CQRS (MediatR) + Validation + Mapping
            // ===============================
            builder.Services.AddMediatR(typeof(CreateAppointmentCommandHandler).Assembly);

            builder.Services.AddAutoMapper(typeof(AppointmentProfile).Assembly);

            builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(FluentValidationBehavior<,>));

            builder.Services.AddValidatorsFromAssemblyContaining<CreateAppointmentCommandValidator>();

            builder.Services.AddScoped<IAppointmentArabicFormatter, ArabicAppointmentFormatter>();

            builder.Services.AddJwtAuthentication(builder.Configuration);
            builder.Services.AddSwaggerServices();

            // ========== Rate Limiting ==========
            builder.Services.AddRateLimiter(options =>
            {
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "global",
                        factory: key => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 100,
                            Window = TimeSpan.FromMinutes(1)
                        }));

                options.RejectionStatusCode = 429;

                // Optional: Policy for sensitive endpoint
                options.AddPolicy("LoginPolicy", context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "global",
                        factory: key => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 5, // 5 login attempts per minute
                            Window = TimeSpan.FromMinutes(1)
                        }));
            });

            // API Versioning
            builder.Services.AddApiVersioning(options =>
            {
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.ReportApiVersions = true;
            });

            builder.Services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
            });

            // CORS Policy
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("DefaultPolicy", policy =>
                {
                    var allowedOrigins = builder.Configuration["AllowedOrigins"]?.Split(',') ?? new[] { "http://localhost:3000" };
                    policy.WithOrigins(allowedOrigins)
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            // Health Checks
            builder.Services.AddHealthChecks()
                .AddSqlServer(builder.Configuration["PersistenceConnectionString"] ?? builder.Configuration.GetConnectionString("PersistenceConnection")!, name: "PersistenceDB")
                .AddSqlServer(builder.Configuration["IdentityConnectionString"] ?? builder.Configuration.GetConnectionString("IdentityConnection")!, name: "IdentityDB");

            var app = builder.Build();

            // ===============================
            // Middleware
            // ===============================
            app.UseResponseCompression();

            // Security Headers
            app.Use(async (context, next) =>
            {
                context.Response.Headers.Append("X-Frame-Options", "DENY");
                context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
                context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
                context.Response.Headers.Append("Referrer-Policy", "no-referrer");
                context.Response.Headers.Append("Content-Security-Policy", "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'");
                await next();
            });

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            if (!app.Environment.IsDevelopment())
            {
                app.UseHsts();
            }
            
            app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseCors("DefaultPolicy");
            app.UseGlobalExceptionHandler();

            app.UseAuthentication();
            app.UseAuthorization();

            // ========== Rate Limiter Middleware ==========
            app.UseRateLimiter();

            // ========== Health Check Endpoint ==========
            app.MapHealthChecks("/health");

            app.MapControllers();

            // ===============================
            // Database Initialization
            // ===============================
            await app.InitializeDatabasesAsync();

            app.Run();
        }
    }
}