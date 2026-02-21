using System.Net;
using System.Text.Json;
using Morourak.API.Errors;

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
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            HttpStatusCode statusCode = HttpStatusCode.InternalServerError;
            string message = "An unexpected error occurred.";

            switch (ex)
            {
                case ArgumentException:
                    statusCode = HttpStatusCode.BadRequest;
                    message = ex.Message;
                    break;

                case InvalidOperationException:
                    statusCode = HttpStatusCode.BadRequest;
                    message = ex.Message;
                    break;

                case KeyNotFoundException:
                    statusCode = HttpStatusCode.NotFound;
                    message = ex.Message;
                    break;

                default:
                    message = "Internal server error";
                    break;
            }

            var response = new ApiErrorResponse
            {
                StatusCode = (int)statusCode,
                Message = message,
                Details = ex.InnerException?.Message
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            return context.Response.WriteAsync(
                JsonSerializer.Serialize(response));
        }
    }
}