using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Morourak.API.Errors;
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

                        var response = new ApiErrorResponse
                        {
                            IsSuccess = false,
                            ErrorCode = "VALIDATION_ERROR",
                            Message = "One or more validation errors occurred.",
                            Details = details,
                            TraceId = context.HttpContext.TraceIdentifier
                        };

                        return new BadRequestObjectResult(response);
                    };
                });

            builder.Services.AddMemoryCache();
            builder.Services.AddApplicationServices(builder.Configuration, builder.Environment);
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
