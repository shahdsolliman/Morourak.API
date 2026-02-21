using System.Threading.Tasks;
using Morourak.Domain.Enums.Request;

namespace Morourak.Application.Interfaces.Services;
public interface IRequestNumberGenerator
{
    Task<string> GenerateAsync(ServiceType serviceType);
}

