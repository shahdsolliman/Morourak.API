using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Morourak.Application.DTOs.Auth;
using Morourak.Application.DTOs.Auth.Arabic;
using Morourak.Application.Interfaces.Services;
using Morourak.Infrastructure.Identity;
using Morourak.Infrastructure.Identity.Constants;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Morourak.API.Controllers
{
    /// <summary>
    /// Controller for handling user authentication, registration, and password management.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Tags("Authentication")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICitizenRegistryService _citizenRegistryService;
        private readonly IConfiguration _configuration;
        private readonly IOtpService _otpService;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            ICitizenRegistryService citizenRegistryService,
            IConfiguration configuration,
            IOtpService otpService)
        {
            _userManager = userManager;
            _citizenRegistryService = citizenRegistryService;
            _configuration = configuration;
            _otpService = otpService;
        }

        // ================= REGISTER =================

        /// <summary>
        /// Registers a new citizen account.
        /// </summary>
        /// <remarks>
        /// This endpoint performs a full match validation against the citizen registry using National ID, 
        /// names, and mobile number. If a match is found and the account doesn't already exist, 
        /// a new user is created and a verification OTP is sent to the registered email.
        /// </remarks>
        /// <param name="request">The registration details.</param>
        /// <response code="200">Registration successful, OTP sent.</response>
        /// <response code="400">Validation failed or citizen registry match failed.</response>
        /// <response code="409">User with same identity or email already exists.</response>
        [HttpPost("register")]
        [ProducesResponseType(typeof(نتيجة_التوثيقDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(نتيجة_التوثيقDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(نتيجة_التوثيقDto), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Register([FromBody] طلب_تسجيل_حساب_جديدDto request)
        {
            var matchResult = await _citizenRegistryService.ValidateFullMatchAsync(
                request.الرقم_القومي,
                request.الاسم_الأول,
                request.الاسم_الأخير,
                request.رقم_الموبايل);

            if (!matchResult.IsMatch)
                return BadRequest(new نتيجة_التوثيقDto
                {
                    ناجح = false,
                    الرسالة = matchResult.Message
                });

            if (await _userManager.FindByEmailAsync(request.البريد_الإلكتروني) != null)
                return Conflict(new نتيجة_التوثيقDto
                {
                    ناجح = false,
                    الرسالة = "البريد الإلكتروني مسجل بالفعل."
                });

            if (_userManager.Users.Any(u => u.NationalId == request.الرقم_القومي))
                return Conflict(new نتيجة_التوثيقDto
                {
                    ناجح = false,
                    الرسالة = "يوجد حساب مسجل بالفعل لهذا الرقم القومي."
                });

            var user = new ApplicationUser
            {
                UserName = request.اسم_المستخدم,
                Email = request.البريد_الإلكتروني,
                PhoneNumber = NormalizePhoneNumber(request.رقم_الموبايل),
                FirstName = request.الاسم_الأول,
                LastName = request.الاسم_الأخير,
                NationalId = request.الرقم_القومي,
                IsVerified = false
            };

            var result = await _userManager.CreateAsync(user, request.كلمة_المرور);

            if (!result.Succeeded)
                return BadRequest(new نتيجة_التوثيقDto
                {
                    ناجح = false,
                    الرسالة = "فشل في إنشاء حساب المستخدم."
                });

            await _userManager.AddToRoleAsync(user, AppIdentityConstants.Roles.Citizen);
            await _otpService.GenerateAndSendAsync(user.Email!, OtpType.Register);

            return Ok(new نتيجة_التوثيقDto
            {
                ناجح = true,
                الرسالة = "تم إرسال رمز التحقق إلى بريدك الإلكتروني."
            });
        }

        // ================= VERIFY OTP =================

        /// <summary>
        /// Verifies the OTP code for account activation or password reset.
        /// </summary>
        /// <param name="dto">The email and 6-digit verification code.</param>
        /// <response code="200">OTP verified successfully.</response>
        /// <response code="400">Invalid OTP or verification failed.</response>
        /// <response code="401">User not found.</response>
        [HttpPost("verify-otp")]
        [ProducesResponseType(typeof(نتيجة_التوثيقDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(نتيجة_التوثيقDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> VerifyOtp([FromBody] التحقق_من_رمز_التفعيلDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.البريد_الإلكتروني);
            if (user == null) return Unauthorized();

            var isValid = await _otpService.ValidateAsync(dto.البريد_الإلكتروني, dto.الرمز);
            if (!isValid) return BadRequest(new نتيجة_التوثيقDto { ناجح = false, الرسالة = "رمز التحقق غير صحيح." });

            user.IsVerified = true;
            await _userManager.UpdateAsync(user);

            return Ok(new نتيجة_التوثيقDto { ناجح = true, الرسالة = "تم تفعيل الحساب بنجاح." });
        }

        // ================= LOGIN =================

        /// <summary>
        /// Authenticates a user and returns security tokens.
        /// </summary>
        /// <remarks>
        /// Validates mobile number and password. If successful, returns a JWT access token 
        /// and a refresh token. The account must be verified prior to login.
        /// </remarks>
        /// <param name="request">Login credentials.</param>
        /// <response code="200">Authentication successful.</response>
        /// <response code="401">Invalid credentials or account deleted.</response>
        /// <response code="400">Account not verified.</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(نتيجة_التوثيقDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(نتيجة_التوثيقDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(نتيجة_التوثيقDto), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] طلب_تسجيل_الدخولDto request)
        {
            var mobileNumber = NormalizePhoneNumber(request.رقم_الموبايل);

            var user = _userManager.Users.FirstOrDefault(u => u.PhoneNumber == mobileNumber);

            if (user == null || user.IsDeleted)
                return Unauthorized(new نتيجة_التوثيقDto { ناجح = false, الرسالة = "بيانات الدخول غير صحيحة." });

            if (!await _userManager.CheckPasswordAsync(user, request.كلمة_المرور))
                return Unauthorized(new نتيجة_التوثيقDto { ناجح = false, الرسالة = "بيانات الدخول غير صحيحة." });

            if (!user.IsVerified)
                return BadRequest(new نتيجة_التوثيقDto { ناجح = false, الرسالة = "الحساب غير مفعل. يرجى تفعيل الحساب أولاً." });

            var response = await CreateTokenResponse(user);
            response.الرسالة = "تم تسجيل الدخول بنجاح";

            return Ok(response);
        }

        // ================= REFRESH TOKEN =================

        /// <summary>
        /// Obtains a new access token using a refresh token.
        /// </summary>
        /// <param name="dto">The refresh token.</param>
        /// <response code="200">Tokens refreshed successfully.</response>
        /// <response code="401">Invalid or expired refresh token.</response>
        [HttpPost("refresh-token")]
        [ProducesResponseType(typeof(نتيجة_التوثيقDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(نتيجة_التوثيقDto), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RefreshToken([FromBody] طلب_توكينDto dto)
        {
            var user = _userManager.Users.FirstOrDefault(u => u.RefreshToken == dto.RefreshToken);

            if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                return Unauthorized(new نتيجة_التوثيقDto { ناجح = false, الرسالة = "رمز التحديث غير صالح." });

            var response = await CreateTokenResponse(user);
            return Ok(response);
        }

        // ================= FORGOT PASSWORD (FROM TOKEN) =================

        /// <summary>
        /// Requests a password reset OTP for the currently logged-in user.
        /// </summary>
        /// <response code="200">OTP sent to email.</response>
        /// <response code="401">Unauthorized access.</response>
        [Authorize]
        [HttpPost("forgot-password")]
        [ProducesResponseType(typeof(نتيجة_التوثيقDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ForgotPassword()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (email == null) return Unauthorized();

            await _otpService.GenerateAndSendAsync(email, OtpType.ResetPassword);

            return Ok(new نتيجة_التوثيقDto
            {
                ناجح = true,
                الرسالة = "تم إرسال رمز إعادة تعيين كلمة المرور إلى بريدك الإلكتروني."
            });
        }

        // ================= RESET PASSWORD (FROM TOKEN) =================

        /// <summary>
        /// Resets the password for the current user using an OTP verification.
        /// </summary>
        /// <param name="request">OTP details and new password.</param>
        /// <response code="200">Password reset successful.</response>
        /// <response code="400">Invalid OTP or password reset failed.</response>
        /// <response code="401">Unauthorized or user not found.</response>
        [Authorize]
        [HttpPost("reset-password")]
        [ProducesResponseType(typeof(نتيجة_التوثيقDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(نتيجة_التوثيقDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ResetPassword([FromBody] إعادة_تعيين_كلمة_المرورDto request)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (email == null) return Unauthorized();

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return Unauthorized();

            var isValid = await _otpService.ValidateAsync(email, request.الرمز);
            if (!isValid) return BadRequest(new نتيجة_التوثيقDto { ناجح = false, الرسالة = "رمز التحقق غير صحيح." });

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, resetToken, request.كلمة_المرور_الجديدة);

            if (!result.Succeeded)
                return BadRequest(new نتيجة_التوثيقDto { ناجح = false, الرسالة = "فشل في إعادة تعيين كلمة المرور." });

            return Ok(new نتيجة_التوثيقDto { ناجح = true, الرسالة = "تم إعادة تعيين كلمة المرور بنجاح." });
        }

        // ================= HELPERS =================

        private async Task<نتيجة_التوثيقDto> CreateTokenResponse(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);

            var accessToken = GenerateJwtToken(user, roles);
            var refreshToken = GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            await _userManager.UpdateAsync(user);

            return new نتيجة_التوثيقDto
            {
                ناجح = true,
                التوكين = accessToken,
                رمز_التحديث = refreshToken,
                الأدوار = roles.ToList()
            };
        }

        private string GenerateJwtToken(ApplicationUser user, IList<string> roles)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim("NationalId", user.NationalId),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expires = DateTime.UtcNow.AddMinutes(
                Convert.ToDouble(_configuration["Jwt:ExpireMinutes"])
            );

            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private static string NormalizePhoneNumber(string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return string.Empty;

            phone = phone.Replace(" ", "").Trim();

            if (phone.StartsWith("+20"))
                phone = "0" + phone.Substring(3);

            return phone;
        }
    }
}
