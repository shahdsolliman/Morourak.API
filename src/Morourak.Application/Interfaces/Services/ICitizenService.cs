using System.Threading.Tasks;
using Morourak.Domain.Entities;

namespace Morourak.Application.Interfaces.Services
{
    /// <summary>
    /// Read operations for citizen data.
    /// Backed by the CitizenRegistry entity in the current domain model.
    /// </summary>
    public interface ICitizenService
    {
        Task<CitizenRegistry?> GetByIdAsync(int id);
    }
}

