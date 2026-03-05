using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Morourak.Application.DTOs.Admin;
using AppEx = Morourak.Application.Exceptions;
using Morourak.Application.Interfaces.Services;
using Morourak.Infrastructure.Identity.Constants;

namespace Morourak.API.Controllers;

[Authorize(Roles = AppIdentityConstants.Roles.Admin)]
[ApiController]
[Route("api/admin/users")]
public class AdminUsersController : ControllerBase
{
    private readonly IAdminUserService _adminUserService;

    public AdminUsersController(IAdminUserService adminUserService)
    {
        _adminUserService = adminUserService;
    }

    [HttpGet]
    public async Task<IActionResult> GetDocs([FromQuery] UserFilterDto filter)
    {
        var result = await _adminUserService.GetUsersAsync(filter);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = AppIdentityConstants.Roles.Admin)]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
    {
        var result = await _adminUserService.CreateUserAsync(dto);
        if (!result.Success)
        {
            if (result.Message != null && result.Message.Contains("exists", StringComparison.OrdinalIgnoreCase))
                throw new AppEx.ValidationException(result.Message, "CONFLICT");
            
            throw new AppEx.ValidationException(result.Message ?? "Failed to create user.");
        }

        return CreatedAtAction(nameof(GetDocs), new { search = dto.Email }, result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = AppIdentityConstants.Roles.Admin)]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateUserDto dto)
    {
        var result = await _adminUserService.UpdateUserAsync(id, dto);
        if (!result.Success)
        {
            if (result.Message != null && result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                throw new AppEx.ValidationException(result.Message, "NOT_FOUND");
                
            throw new AppEx.ValidationException(result.Message ?? "Failed to update user.");
        }

        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = AppIdentityConstants.Roles.Admin)]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _adminUserService.DeleteUserAsync(id);
        if (!result.Success)
        {
            throw new AppEx.ValidationException(result.Message ?? "User not found.", "NOT_FOUND");
        }

        return Ok(new { IsSuccess = true, Message = "User deleted successfully." });
    }
}
