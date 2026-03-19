using Morourak.Application.Exceptions;
using Morourak.Domain.Entities;

namespace Morourak.Application.DTOs.Auth
{
    public class CitizenMatchResult
    {
        public bool IsMatch { get; set; }

        public List<ErrorDetail> Errors { get; set; } = new();
        public CitizenRegistry? Citizen { get; set; }

        public string Message =>
            IsMatch ? "Match successful" : "Provided data does not match official records";
    }
}
