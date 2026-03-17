using System.Net;
using System.Text.Json;
using Morourak.API.Common;
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
            var response = ApiResponseArabic.Fail(
                ex.Message,
                ex.ErrorCode,
                ex.Details
            );

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
        }

        private async Task HandleSystemExceptionAsync(HttpContext context, Exception ex)
        {
            var response = ApiResponseArabic.Fail(
                "حدث خطأ غير متوقع. يرجى المحاولة لاحقاً أو التواصل مع الدعم الفني.",
                "SERVER_ERROR"
            );

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
        }
    }
}
