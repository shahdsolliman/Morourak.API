using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Morourak.Application.Common;
using Morourak.Application.DTOs.Admin;
using Morourak.Application.Interfaces.Services;
using Morourak.Infrastructure.Identity;
using Morourak.Infrastructure.Identity.Constants;

namespace Morourak.Infrastructure.Services;

public class AdminUserService : IAdminUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AdminUserService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<PagedResponse<List<UserDto>>> GetUsersAsync(UserFilterDto filter)
    {
        var query = _userManager.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            query = query.Where(u => 
                u.Email.Contains(filter.Search) || 
                u.FirstName.Contains(filter.Search) || 
                u.LastName.Contains(filter.Search));
        }

        if (filter.IsActive.HasValue)
        {
            query = query.Where(u => u.IsActive == filter.IsActive.Value);
        }

        query = filter.SortBy switch
        {
            "Email" => filter.IsDescending ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email),
            "Name" => filter.IsDescending ? query.OrderByDescending(u => u.FirstName) : query.OrderBy(u => u.FirstName),
            _ => filter.IsDescending ? query.OrderByDescending(u => u.CreatedAt) : query.OrderBy(u => u.CreatedAt)
        };

        var totalRecords = await query.CountAsync();
        var users = await query
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        var userDtos = new List<UserDto>();
        foreach (var user in users)
        {
            var userRoles = await _userManager.GetRolesAsync(user);
            userDtos.Add(new UserDto
            {
                Id = user.Id,
                Name = $"{user.FirstName} {user.LastName}",
                Email = user.Email!,
                Role = userRoles.FirstOrDefault() ?? "None",
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            });
        }

        return new PagedResponse<List<UserDto>>(userDtos, filter.PageNumber, filter.PageSize, totalRecords);
    }

    public async Task<ApiResponse<UserDto>> CreateUserAsync(CreateUserDto dto)
    {
        if (await _userManager.FindByEmailAsync(dto.Email) != null)
            return ApiResponse<UserDto>.FailureResult("Email already exists.");

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            IsActive = dto.IsActive,
            IsVerified = true,
            NationalId = "00000000000000"
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return ApiResponse<UserDto>.FailureResult("Failed to create user.", result.Errors.Select(e => e.Description).ToList());

        if (!await _roleManager.RoleExistsAsync(dto.Role))
            return ApiResponse<UserDto>.FailureResult($"Role '{dto.Role}' does not exist.");

        await _userManager.AddToRoleAsync(user, dto.Role);

        return ApiResponse<UserDto>.SuccessResult(new UserDto
        {
            Id = user.Id,
            Name = $"{user.FirstName} {user.LastName}",
            Email = user.Email,
            Role = dto.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        }, "User created successfully.");
    }

    public async Task<ApiResponse<UserDto>> UpdateUserAsync(string id, UpdateUserDto dto)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return ApiResponse<UserDto>.FailureResult("User not found.");

        if (dto.FirstName != null) user.FirstName = dto.FirstName;
        if (dto.LastName != null) user.LastName = dto.LastName;
        if (dto.IsActive.HasValue) user.IsActive = dto.IsActive.Value;

        if (dto.Email != null && dto.Email != user.Email)
        {
            if (await _userManager.FindByEmailAsync(dto.Email) != null)
                return ApiResponse<UserDto>.FailureResult("Email already exists.");
            
            user.Email = dto.Email;
            user.UserName = dto.Email;
        }

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return ApiResponse<UserDto>.FailureResult("Failed to update user.");

        if (dto.Role != null)
        {
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, dto.Role);
        }

        var userRoles = await _userManager.GetRolesAsync(user);

        return ApiResponse<UserDto>.SuccessResult(new UserDto
        {
            Id = user.Id,
            Name = $"{user.FirstName} {user.LastName}",
            Email = user.Email!,
            Role = userRoles.FirstOrDefault() ?? "None",
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        }, "User updated successfully.");
    }

    public async Task<ApiResponse<bool>> DeleteUserAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return ApiResponse<bool>.FailureResult("User not found.");

        // Protection: Prevent deletion of primary Admin
        var roles = await _userManager.GetRolesAsync(user);
        if (roles.Contains(AppIdentityConstants.Roles.Admin) && user.Email == "admin@morourak.com")
            return ApiResponse<bool>.FailureResult("Primary administrator cannot be deleted.");

        user.IsDeleted = true;
        user.DeletedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded 
            ? ApiResponse<bool>.SuccessResult(true, "User soft-deleted successfully.")
            : ApiResponse<bool>.FailureResult("Failed to delete user.");
    }
}
