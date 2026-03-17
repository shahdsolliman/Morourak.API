using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Morourak.Application.DTOs.Admin;
using AppEx = Morourak.Application.Exceptions;
using Morourak.Application.Interfaces.Services;
using Morourak.Infrastructure.Identity.Constants;
using Morourak.API.Errors;

namespace Morourak.API.Controllers;

/// <summary>
/// Controller for administrators to manage system users and staff.
/// </summary>
/// <response code="401">Unauthorized: Authentication is required.</response>
/// <response code="403">Forbidden: User must have Admin role.</response>
[Authorize(Roles = AppIdentityConstants.Roles.Admin)]
[ApiController]
[Route("api/admin/users")]
[Tags("User Management")]
public class AdminUsersController : ControllerBase
{
    private readonly IAdminUserService _adminUserService;

    public AdminUsersController(IAdminUserService adminUserService)
    {
        _adminUserService = adminUserService;
    }

    /// <summary>
    /// Retrieves a paginated list of users based on filter criteria.
    /// </summary>
    /// <param name="filter">Search, sort, and pagination parameters.</param>
    /// <returns>A collection of UserDto objects.</returns>
    /// <response code="200">Users retrieved successfully.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDocs([FromQuery] UserFilterDto filter)
    {
        var result = await _adminUserService.GetUsersAsync(filter);
        return Ok(result);
    }

    /// <summary>
    /// Creates a new user or staff member.
    /// </summary>
    /// <param name="dto">The information for the new user.</param>
    /// <response code="201">User created successfully.</response>
    /// <response code="400">Validation error or user already exists.</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
    {
        var result = await _adminUserService.CreateUserAsync(dto);
        if (!result.Success)
        {
            if (result.Message != null && result.Message.Contains("exists", StringComparison.OrdinalIgnoreCase))
                throw new AppEx.ValidationException(result.Message, "CONFLICT");
            
            throw new AppEx.ValidationException(result.Message ?? "فشل في إنشاء المستخدم.");
        }

        return CreatedAtAction(nameof(GetDocs), new { search = dto.Email }, result);
    }

    /// <summary>
    /// Updates an existing user's profile and settings.
    /// </summary>
    /// <param name="id">The unique identifier of the user to update.</param>
    /// <param name="dto">The updated information.</param>
    /// <response code="200">User updated successfully.</response>
    /// <response code="404">User not found.</response>
    /// <response code="400">Update failed due to validation issues.</response>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateUserDto dto)
    {
        var result = await _adminUserService.UpdateUserAsync(id, dto);
        if (!result.Success)
        {
            if (result.Message != null && result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                throw new AppEx.ValidationException(result.Message, "NOT_FOUND");
                
            throw new AppEx.ValidationException(result.Message ?? "فشل في تحديث بيانات المستخدم.");
        }

        return Ok(result);
    }

    /// <summary>
    /// Permanently deletes a user account.
    /// </summary>
    /// <param name="id">The unique identifier of the user to delete.</param>
    /// <response code="200">User deleted successfully.</response>
    /// <response code="404">User not found.</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _adminUserService.DeleteUserAsync(id);
        if (!result.Success)
        {
            throw new AppEx.ValidationException(result.Message ?? "المستخدم غير موجود.", "NOT_FOUND");
        }

        return Ok(new { IsSuccess = true, Message = "User deleted successfully." });
    }
}
