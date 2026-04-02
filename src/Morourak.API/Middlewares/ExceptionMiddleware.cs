using System.Net;
using System.Text.Json;
using Morourak.Application.Exceptions;

namespace Morourak.API.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IWebHostEnvironment _environment;

        public ExceptionMiddleware(
            RequestDelegate next,
            ILogger<ExceptionMiddleware> logger,
            IWebHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "INVALID_JSON: {Message}", ex.Message);
                await HandleJsonExceptionAsync(context, ex);
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

        private static async Task HandleJsonExceptionAsync(HttpContext context, JsonException ex)
        {
            var response = new
            {
                isSuccess = false,
                message = "بيانات غير صالحة.",
                errorCode = "VALIDATION_ERROR",
                details = new[]
                {
                    new ErrorDetail
                    {
                        Field = "body",
                        Error = ex.Message
                    }
                }
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
        }

        private async Task HandleBusinessExceptionAsync(HttpContext context, AppException ex)
        {
            var response = new
            {
                isSuccess = false,
                message = ex.Message,
                errorCode = ex.ErrorCode,
                details = ex.Details
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
        }

        private async Task HandleSystemExceptionAsync(HttpContext context, Exception ex)
        {
            object response = _environment.IsDevelopment()
                ? new
                {
                    isSuccess = false,
                    message = "حدث خطأ غير متوقع. يرجى المحاولة لاحقاً أو التواصل مع الدعم الفني.",
                    errorCode = "SERVER_ERROR",
                    debugMessage = ex.Message,
                    stackTrace = ex.StackTrace
                }
                : new
                {
                    isSuccess = false,
                    message = "حدث خطأ غير متوقع. يرجى المحاولة لاحقاً أو التواصل مع الدعم الفني.",
                    errorCode = "SERVER_ERROR"
                };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
        }
    }
}
