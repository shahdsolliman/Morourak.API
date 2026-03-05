using System.Collections.Generic;

namespace Morourak.Application.Exceptions
{
    public class BusinessValidationException : AppException
    {
        public BusinessValidationException(string message, string errorCode = "BUSINESS_VALIDATION", List<ErrorDetail>? details = null)
            : base(message, errorCode, details)
        {
        }
    }
}