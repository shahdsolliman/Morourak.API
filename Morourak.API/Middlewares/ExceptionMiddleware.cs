using System.Net;
using System.Text.Json;
using Morourak.API.Errors;
using Morourak.Application.Exceptions;

namespace Morourak.API.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (AppException ex)
            {
                _logger.LogWarning("BUSINESS_ERROR: {ErrorCode} - {Message}", ex.ErrorCode, ex.Message);
                await HandleBusinessExceptionAsync(context, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SYSTEM_ERROR: An unhandled exception has occurred: {Message}", ex.Message);
                await HandleSystemExceptionAsync(context, ex);
            }
        }

        private async Task HandleBusinessExceptionAsync(HttpContext context, AppException ex)
        {
            var response = new ApiErrorResponse
            {
                IsSuccess = false,
                ErrorCode = ex.ErrorCode,
                Message = ex.Message,
                Details = ex.Details,
                TraceId = context.TraceIdentifier
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
        }

        private async Task HandleSystemExceptionAsync(HttpContext context, Exception ex)
        {
            var response = new ApiErrorResponse
            {
                IsSuccess = false,
                ErrorCode = "SERVER_ERROR",
                Message = "An unexpected error occurred. Please try again later or contact support.",
                TraceId = context.TraceIdentifier
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
        }
    }
}
