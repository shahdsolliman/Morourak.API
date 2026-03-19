using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Morourak.Application.DTOs.Auth;
using Morourak.Application.Interfaces.Services;
using Morourak.Infrastructure.Identity;
using System.Security.Claims;

namespace Morourak.API.Controllers
{
    /// <summary>
    /// Controller for handling user authentication, registration, and password management.
    /// Redirects core logic to IIdentityService.
    /// </summary>
    [Route("api/v1/[controller]")]
    [Tags("Authentication")]
    public class AuthController : BaseApiController
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IIdentityService _identityService;
        private readonly IOtpService _otpService;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            IIdentityService identityService,
            IOtpService otpService)
        {
            _userManager = userManager;
            _identityService = identityService;
            _otpService = otpService;
        }

        // ================= REGISTER =================

        /// <summary>
        /// Registers a new citizen account.
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            var result = await _identityService.RegisterAsync(request);
            
            if (!result.IsSuccess)
                return BadRequest(new
                {
                    isSuccess = false,
                    message = result.Message,
                    errorCode = result.ErrorCode,
                });

            return Ok(new
            {
                isSuccess = true,
                message = result.Message,
                details = (object?)null
            });
        }

        // ================= VERIFY OTP =================

        /// <summary>
        /// Verifies the OTP code for account activation or password reset.
        /// </summary>
        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null) return Unauthorized();

            var isValid = await _otpService.ValidateAsync(dto.Email, dto.Code);
            if (!isValid)
                return BadRequest(new
                {
                    isSuccess = false,
                    message = "رمز التحقق غير صحيح.",
                    errorCode = "INVALID_OTP",
                });

            user.IsVerified = true;
            await _userManager.UpdateAsync(user);

            return Ok(new
            {
                isSuccess = true,
                message = "تم تفعيل الحساب بنجاح.",
                details = (object?)null
            });
        }

        // ================= LOGIN =================

        /// <summary>
        /// Authenticates a user and returns security tokens.
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            var result = await _identityService.LoginAsync(request.MobileNumber, request.Password);

            if (!result.IsSuccess)
                return Unauthorized(new
                {
                    isSuccess = false,
                    message = result.Message,
                    errorCode = result.ErrorCode,
                });

            return Ok(new
            {
                isSuccess = true,
                message = "تم تسجيل الدخول بنجاح",
                details = result
            });
        }

        // ================= REFRESH TOKEN =================

        /// <summary>
        /// Obtains a new access token using a refresh token.
        /// </summary>
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenRequestDto dto)
        {
            var result = await _identityService.RefreshTokenAsync(dto.RefreshToken);

            if (!result.IsSuccess)
                return Unauthorized(new
                {
                    isSuccess = false,
                    message = result.Message,
                    errorCode = result.ErrorCode,
                });

            return Ok(new
            {
                isSuccess = true,
                message = "تم تحديث التوكين بنجاح",
                details = result
            });
        }

        // ================= FORGOT PASSWORD (FROM TOKEN) =================

        /// <summary>
        /// Requests a password reset OTP for the currently logged-in user.
        /// </summary>
        [Authorize]
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (email == null) return Unauthorized();

            await _otpService.GenerateAndSendAsync(email, OtpType.ResetPassword);

            return Ok(new
            {
                isSuccess = true,
                message = "تم إرسال رمز إعادة تعيين كلمة المرور إلى بريدك الإلكتروني.",
                details = (object?)null
            });
        }

        // ================= RESET PASSWORD (FROM TOKEN) =================

        /// <summary>
        /// Resets the password for the current user using an OTP verification.
        /// </summary>
        [Authorize]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto request)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (email == null) return Unauthorized();

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return Unauthorized();

            var isValid = await _otpService.ValidateAsync(email, request.Code);
            if (!isValid)
                return BadRequest(new
                {
                    isSuccess = false,
                    message = "رمز التحقق غير صحيح.",
                    errorCode = "INVALID_OTP",
                });

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, resetToken, request.NewPassword);

            if (!result.Succeeded)
                return BadRequest(new
                {
                    isSuccess = false,
                    message = "فشل في إعادة تعيين كلمة المرور.",
                    errorCode = "RESET_FAILED",
                });

            return Ok(new
            {
                isSuccess = true,
                message = "تم إعادة تعيين كلمة المرور بنجاح.",
                details = (object?)null
            });
        }
    }
}
