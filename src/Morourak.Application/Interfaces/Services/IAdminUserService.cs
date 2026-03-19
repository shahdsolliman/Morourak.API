using Morourak.Application.Common;
using Morourak.Application.DTOs.Admin;

namespace Morourak.Application.Interfaces.Services;

public interface IAdminUserService
{
    Task<PagedResponse<List<UserDto>>> GetUsersAsync(UserFilterDto filter);
    Task<ApiResponse<UserDto>> CreateUserAsync(CreateUserDto dto);
    Task<ApiResponse<UserDto>> UpdateUserAsync(string id, UpdateUserDto dto);
    Task<ApiResponse<bool>> DeleteUserAsync(string id);
}
