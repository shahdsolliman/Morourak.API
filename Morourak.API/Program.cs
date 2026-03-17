using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Morourak.API.Common;
using Morourak.API.Extensions;
using Morourak.API.Extensions.JsonConverters;
using Morourak.API.Middleware;
using Morourak.Application.Exceptions;
using Morourak.Infrastructure.Settings;
using System.Text.Json.Serialization;

namespace Morourak.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ===============================
            // Services
            // ===============================
            builder.Services
                .AddControllers()
                .AddJsonOptions(options =>
                {
                    // Global Arabic Enum Converter
                    options.JsonSerializerOptions.Converters.Add(
                        new ArabicEnumConverter());

                    // Support DateOnly
                    options.JsonSerializerOptions.Converters.Add(
                        new DateOnlyJsonConverter());

                    // Support TimeOnly
                    options.JsonSerializerOptions.Converters.Add(
                        new TimeOnlyJsonConverter());
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

                        var response = ApiResponseArabic.ValidationFail(details);

                        return new BadRequestObjectResult(response);
                    };
                });

            builder.Services.AddMemoryCache();
            builder.Services.AddApplicationServices(builder.Configuration, builder.Environment);
            builder.Services.AddJwtAuthentication(builder.Configuration);
            builder.Services.AddSwaggerServices();
            
            // API Versioning
            builder.Services.AddApiVersioning(options =>
            {
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.ReportApiVersions = true;
            });

            // CORS Policy
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("DefaultPolicy", policy =>
                {
                    policy.WithOrigins(builder.Configuration["AllowedOrigins"]?.Split(',') ?? new[] { "http://localhost:3000" })
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            var app = builder.Build();

            // ===============================
            // Middleware
            // ===============================
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            if (!app.Environment.IsDevelopment())
            {
                app.UseHsts();
                app.UseHttpsRedirection();
            }
            else
            {
                var urls = app.Configuration["ASPNETCORE_URLS"];
                var hasHttpsUrl = urls?.Contains("https://", StringComparison.OrdinalIgnoreCase) == true;
                var httpsPort = app.Configuration.GetValue<int?>("ASPNETCORE_HTTPS_PORT");

                if (hasHttpsUrl || httpsPort.HasValue)
                    app.UseHttpsRedirection();
            }
            app.UseStaticFiles();
            app.UseCors("DefaultPolicy");
            app.UseGlobalExceptionHandler();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            // ===============================
            // Database Initialization
            // ===============================
            await app.InitializeDatabasesAsync();

            app.Run();
        }
    }
}
