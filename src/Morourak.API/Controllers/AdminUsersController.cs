using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Morourak.Application.DTOs.Admin;
using AppEx = Morourak.Application.Exceptions;
using Morourak.Application.Interfaces.Services;
using Morourak.Infrastructure.Identity.Constants;

namespace Morourak.API.Controllers;

/// <summary>
/// Controller for administrators to manage system users and staff.
/// </summary>
[Authorize(Roles = AppIdentityConstants.Roles.Admin)]
[Route("api/v1/[controller]")]
[Tags("User Management")]
public class AdminUsersController : BaseApiController
{
    private readonly IAdminUserService _adminUserService;

    public AdminUsersController(IAdminUserService adminUserService)
    {
        _adminUserService = adminUserService;
    }

    /// <summary>
    /// Retrieves a paginated list of users based on filter criteria.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetDocs([FromQuery] Morourak.API.DTOs.Admin.AdminUserFilterApiDto filter)
    {
        var result = await _adminUserService.GetUsersAsync(filter.ToApplicationDto());
        return Ok(new
        {
            isSuccess = true,
            message = (string?)null,
            errorCode = (string?)null,
            details = result
        });
    }

    /// <summary>
    /// Creates a new user or staff member.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
    {
        var result = await _adminUserService.CreateUserAsync(dto);
        if (!result.Success)
        {
            if (result.ErrorCode == "EMAIL_EXISTS")
                throw new AppEx.ValidationException(result.Message ?? "Email already exists.", "CONFLICT");
            
            throw new AppEx.ValidationException(result.Message ?? "فشل في إنشاء المستخدم.");
        }

        return CreatedAtAction(nameof(GetDocs), new { search = dto.Email },
            new
            {
                isSuccess = true,
                message = "تم إنشاء المستخدم بنجاح",
                errorCode = (string?)null,
                details = result
            });
    }

    /// <summary>
    /// Updates an existing user's profile and settings.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateUserDto dto)
    {
        var result = await _adminUserService.UpdateUserAsync(id, dto);
        if (!result.Success)
        {
            if (result.ErrorCode == "USER_NOT_FOUND")
                throw new AppEx.ValidationException(result.Message ?? "User not found.", "NOT_FOUND");
                
            throw new AppEx.ValidationException(result.Message ?? "فشل في تحديث بيانات المستخدم.");
        }

        return Ok(new
        {
            isSuccess = true,
            message = "تم تحديث بيانات المستخدم بنجاح",
            errorCode = (string?)null,
            details = result
        });
    }

    /// <summary>
    /// Permanently deletes a user account.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _adminUserService.DeleteUserAsync(id);
        if (!result.Success)
        {
            if (result.ErrorCode == "USER_NOT_FOUND")
                throw new AppEx.ValidationException(result.Message ?? "User not found.", "NOT_FOUND");

            if (result.ErrorCode == "PRIMARY_ADMIN_PROTECTED")
                throw new AppEx.ValidationException(result.Message ?? "Primary administrator cannot be deleted.", "FORBIDDEN");

            // Anything else is a server-side failure; let global exception middleware return 500.
            throw new Exception(result.Message ?? "Failed to delete user.");
        }

        return Ok(new
        {
            isSuccess = true,
            message = "تم حذف المستخدم بنجاح.",
            errorCode = (string?)null,
            details = (object?)null
        });
    }
}

