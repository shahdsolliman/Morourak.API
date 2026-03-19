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
    public async Task<IActionResult> GetDocs([FromQuery] UserFilterDto filter)
    {
        var result = await _adminUserService.GetUsersAsync(filter);
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
            if (result.Message != null && result.Message.Contains("exists", StringComparison.OrdinalIgnoreCase))
                throw new AppEx.ValidationException(result.Message, "CONFLICT");
            
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
            if (result.Message != null && result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                throw new AppEx.ValidationException(result.Message, "NOT_FOUND");
                
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
            throw new AppEx.ValidationException(result.Message ?? "المستخدم غير موجود.", "NOT_FOUND");
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
