using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Morourak.API.Common;
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
    [ApiController]
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
        [ProducesResponseType(typeof(ApiResponseArabic), StatusCodes.Status200OK)]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            var result = await _identityService.RegisterAsync(request);
            
            if (!result.IsSuccess)
                return BadRequest(ApiResponseArabic.Fail(result.Message, result.ErrorCode));

            return Ok(ApiResponseArabic.Success(null, result.Message));
        }

        // ================= VERIFY OTP =================

        /// <summary>
        /// Verifies the OTP code for account activation or password reset.
        /// </summary>
        [HttpPost("verify-otp")]
        [ProducesResponseType(typeof(ApiResponseArabic), StatusCodes.Status200OK)]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null) return Unauthorized();

            var isValid = await _otpService.ValidateAsync(dto.Email, dto.Code);
            if (!isValid) return BadRequest(ApiResponseArabic.Fail("رمز التحقق غير صحيح.", "INVALID_OTP"));

            user.IsVerified = true;
            await _userManager.UpdateAsync(user);

            return Ok(ApiResponseArabic.Success(null, "تم تفعيل الحساب بنجاح."));
        }

        // ================= LOGIN =================

        /// <summary>
        /// Authenticates a user and returns security tokens.
        /// </summary>
        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponseArabic), StatusCodes.Status200OK)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            var result = await _identityService.LoginAsync(request.MobileNumber, request.Password);

            if (!result.IsSuccess)
                return Unauthorized(ApiResponseArabic.Fail(result.Message, result.ErrorCode));

            return Ok(ApiResponseArabic.Success(result, "تم تسجيل الدخول بنجاح"));
        }

        // ================= REFRESH TOKEN =================

        /// <summary>
        /// Obtains a new access token using a refresh token.
        /// </summary>
        [HttpPost("refresh-token")]
        [ProducesResponseType(typeof(ApiResponseArabic), StatusCodes.Status200OK)]
        public async Task<IActionResult> RefreshToken([FromBody] TokenRequestDto dto)
        {
            var result = await _identityService.RefreshTokenAsync(dto.RefreshToken);

            if (!result.IsSuccess)
                return Unauthorized(ApiResponseArabic.Fail(result.Message, result.ErrorCode));

            return Ok(ApiResponseArabic.Success(result, "تم تحديث التوكين بنجاح"));
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

            return Ok(ApiResponseArabic.Success(null, "تم إرسال رمز إعادة تعيين كلمة المرور إلى بريدك الإلكتروني."));
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
            if (!isValid) return BadRequest(ApiResponseArabic.Fail("رمز التحقق غير صحيح.", "INVALID_OTP"));

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, resetToken, request.NewPassword);

            if (!result.Succeeded)
                return BadRequest(ApiResponseArabic.Fail("فشل في إعادة تعيين كلمة المرور.", "RESET_FAILED"));

            return Ok(ApiResponseArabic.Success(null, "تم إعادة تعيين كلمة المرور بنجاح."));
        }
    }
}
