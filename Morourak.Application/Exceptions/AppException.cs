using System;
using System.Collections.Generic;

namespace Morourak.Application.Exceptions
{
    public abstract class AppException : Exception
    {
        public string ErrorCode { get; }
        public List<ErrorDetail>? Details { get; }

        protected AppException(string message, string errorCode, List<ErrorDetail>? details = null) : base(message)
        {
            ErrorCode = errorCode;
            Details = details;
        }
    }
}
