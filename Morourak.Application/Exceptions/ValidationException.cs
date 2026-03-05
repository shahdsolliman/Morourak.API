using System.Collections.Generic;

namespace Morourak.Application.Exceptions
{
    public class ValidationException : AppException
    {
        public ValidationException(string message)
            : base(message, "VALIDATION_ERROR") { }

        public ValidationException(string message, string errorCode)
            : base(message, errorCode) { }

        public ValidationException(string message, List<ErrorDetail> details)
            : base(message, "VALIDATION_ERROR", details) { }

        public ValidationException(string message, string errorCode, List<ErrorDetail> details)
            : base(message, errorCode, details) { }
    }

    public class ErrorDetail
    {
        public string Field { get; set; }
        public string Error { get; set; }
    }
}
