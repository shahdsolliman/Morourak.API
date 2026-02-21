using Microsoft.Extensions.Configuration;
using Morourak.API.Extensions;
using Morourak.API.Middleware;
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
                    // Allow enums as string or int
                    options.JsonSerializerOptions.Converters.Add(
                        new JsonStringEnumConverter());

                    // Support DateOnly
                    options.JsonSerializerOptions.Converters.Add(
                        new DateOnlyJsonConverter());

                    // Support TimeOnly
                    options.JsonSerializerOptions.Converters.Add(
                        new TimeOnlyJsonConverter());
                });

            builder.Services.AddApplicationServices(builder.Configuration);
            builder.Services.AddJwtAuthentication(builder.Configuration);
            builder.Services.AddSwaggerServices();

            var app = builder.Build();

            // ===============================
            // Middleware
            // ===============================
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
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
