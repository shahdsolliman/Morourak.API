using Microsoft.AspNetCore.Mvc;
using Morourak.Application.Exceptions;
using Morourak.Application.Common;
using System.Security.Claims;

namespace Morourak.API.Controllers;

/// <summary>
/// Base class for all API controllers in Morourak.
/// Centeralizes common logic like identity extraction and shared helpers.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    protected string NationalId
    {
        get
        {
            var nationalId = User.FindFirst("NationalId")?.Value;
            if (string.IsNullOrEmpty(nationalId))
                throw new UnauthorizedException("رقم الهوية غير موجود في رمز التحقق.", "AUTH_MISSING_NATIONAL_ID");
            return nationalId;
        }
    }

    protected string UserId
    {
        get
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedException("معرف المستخدم غير موجود.", "AUTH_MISSING_USER_ID");
            return userId;
        }
    }

    protected IActionResult Success<T>(T data, string? message = null)
        => Ok(new ApiResponse<T> { IsSuccess = true, Details = data, Message = message });

    protected IActionResult SuccessPaginated<T>(IEnumerable<T> data, int page, int pageSize, int total, string? message = null)
        => Ok(new PagedApiResponse<T>(data, page, pageSize, total, message));

    protected async Task<byte[]> ToByteArrayAsync(IFormFile file)
    {
        if (file == null) return Array.Empty<byte>();
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        return ms.ToArray();
    }
}
