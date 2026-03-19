using Morourak.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http; 
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? NationalId =>
        _httpContextAccessor.HttpContext?
        .User?
        .FindFirst("NationalId")?.Value;
}