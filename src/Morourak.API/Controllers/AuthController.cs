using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Morourak.API.DTOs.User;
using Morourak.Application.DTOs.Auth;
using Morourak.Application.Interfaces.Services;
using Morourak.Infrastructure.Identity;
using System.Security.Claims;
using Morourak.Domain.Enums;

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

            return Success((object?)null, result.Message);
        }

        // ================= VERIFY REGISTRATION OTP =================
 
        /// <summary>
        /// Verifies the OTP code for account activation for a new registration.
        /// </summary>
        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto dto)
        {
            var result = await _identityService.ConfirmRegistrationAsync(dto.Email, dto.Code);
            
            if (!result.IsSuccess)
            {
                return BadRequest(new
                {
                    isSuccess = false,
                    message = result.Message,
                    errorCode = result.ErrorCode,
                });
            }
 
            return Success((object?)null, result.Message);
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
 
            return Success(result, "تم تسجيل الدخول بنجاح");
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
 
            return Success(result, "تم تحديث التوكين بنجاح");
        }
 
        // ================= FORGOT PASSWORD (ANONYMOUS) =================
 
        /// <summary>
        /// Requests a password reset OTP for a user who is not logged in.
        /// </summary>
        [AllowAnonymous]
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
        {
            var result = await _identityService.ForgotPasswordAsync(request.Email);
            return Success((object?)null, result.Message);
        }
 
        // ================= RESET PASSWORD (ANONYMOUS) =================
 
        /// <summary>
        /// Resets the password for a user using an OTP verification.
        /// </summary>
        [AllowAnonymous]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto request)
        {
            var result = await _identityService.ResetPasswordAsync(request);
            
            if (!result.IsSuccess)
                return BadRequest(new
                {
                    isSuccess = false,
                    message = result.Message,
                    errorCode = result.ErrorCode,
                });
 
            return Success((object?)null, result.Message);
        }

        // ================= REQUEST CHANGE EMAIL =================

        /// <summary>
        /// Requests an email change by sending an OTP to the new email.
        /// </summary>
        [Authorize]
        [HttpPost("change-email/request")]
        public async Task<IActionResult> RequestChangeEmail([FromBody] ChangeEmailRequestDto request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var result = await _identityService.RequestChangeEmailAsync(userId, request.NewEmail);

            if (!result.IsSuccess)
                return BadRequest(new
                {
                    isSuccess = false,
                    message = result.Message,
                    errorCode = result.ErrorCode,
                });

            return Success((object?)null, result.Message);
        }

        // ================= CONFIRM CHANGE EMAIL =================

        /// <summary>
        /// Confirms the email change using the OTP sent to the new email.
        /// </summary>
        [Authorize]
        [HttpPost("change-email/confirm")]
        public async Task<IActionResult> ConfirmChangeEmail([FromBody] ConfirmChangeEmailDto request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var result = await _identityService.ConfirmChangeEmailAsync(userId, request.NewEmail, request.Code);

            if (!result.IsSuccess)
                return BadRequest(new
                {
                    isSuccess = false,
                    message = result.Message,
                    errorCode = result.ErrorCode,
                });

            return Success((object?)null, result.Message);
        }

        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Unauthorized();

            var result = new CitizenProfileDto
            {
                FullName = $"{user.FirstName} {user.LastName}",
                NationalId = user.NationalId,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email
            };

            return Success(result);
        }
    }
}
