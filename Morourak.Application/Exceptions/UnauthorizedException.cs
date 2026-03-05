using System.Collections.Generic;

namespace Morourak.Application.Exceptions
{
    public class UnauthorizedException : AppException
    {
        public UnauthorizedException(string message, string errorCode = "AUTH_ERROR", List<ErrorDetail>? details = null) 
            : base(message, errorCode, details)
        {
        }
    }
}
