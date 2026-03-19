using System.Collections.Generic;
using Morourak.Application.Exceptions;

namespace Morourak.API.Errors
{
    public class ApiErrorResponse
    {
        public bool IsSuccess { get; set; } = false;
        public string ErrorCode { get; set; } = null!;
        public string Message { get; set; } = null!;
        public List<ErrorDetail>? Details { get; set; }
        public string? TraceId { get; set; }
    }
}