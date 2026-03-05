namespace Morourak.Application.Exceptions
{
    public class NotFoundException : AppException
    {
        public NotFoundException(string message) : base(message, "NOT_FOUND")
        {
        }
    }
}
