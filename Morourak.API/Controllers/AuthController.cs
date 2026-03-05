using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Morourak.Application.DTOs.Auth;
using Morourak.Application.Interfaces.Services;
using Morourak.Infrastructure.Identity;
using Morourak.Infrastructure.Identity.Constants;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Morourak.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            var matchResult = await _citizenRegistryService.ValidateFullMatchAsync(
                request.NationalId,
                request.FirstName,
                request.LastName,
                request.MobileNumber);

            if (!matchResult.IsMatch)
                return BadRequest(new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = matchResult.Message
                });

            if (await _userManager.FindByEmailAsync(request.Email) != null)
                return Conflict(new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "Email already exists."
                });

            if (_userManager.Users.Any(u => u.NationalId == request.NationalId))
                return Conflict(new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "An account already exists for this National ID."
                });

            var user = new ApplicationUser
            {
                UserName = request.Username,
                Email = request.Email,
                PhoneNumber = NormalizePhoneNumber(request.MobileNumber),
                FirstName = request.FirstName,
                LastName = request.LastName,
                NationalId = request.NationalId,
                IsVerified = false
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
                return BadRequest(new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "User creation failed."
                });

            await _userManager.AddToRoleAsync(user, AppIdentityConstants.Roles.Citizen);
            await _otpService.GenerateAndSendAsync(user.Email!, OtpType.Register);

            return Ok(new AuthResponseDto
            {
                IsSuccess = true,
                Message = "Verification code sent to your email."
            });
        }

        // ================= VERIFY OTP =================

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null) return Unauthorized();

            var isValid = await _otpService.ValidateAsync(dto.Email, dto.Code);
            if (!isValid) return BadRequest(new AuthResponseDto { IsSuccess = false, Message = "Invalid code" });

            user.IsVerified = true;
            await _userManager.UpdateAsync(user);

            return Ok(new AuthResponseDto { IsSuccess = true, Message = "Account verified successfully." });
        }

        // ================= LOGIN =================

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            var mobileNumber = NormalizePhoneNumber(request.MobileNumber);

            var user = _userManager.Users.FirstOrDefault(u => u.PhoneNumber == mobileNumber);

            if (user == null || user.IsDeleted)
                return Unauthorized(new AuthResponseDto { IsSuccess = false, Message = "Invalid credentials" });

            if (!await _userManager.CheckPasswordAsync(user, request.Password))
                return Unauthorized(new AuthResponseDto { IsSuccess = false, Message = "Invalid credentials" });

            if (!user.IsVerified)
                return BadRequest(new AuthResponseDto { IsSuccess = false, Message = "Account not verified" });

            var response = await CreateTokenResponse(user);
            response.Message = "Login successful";

            return Ok(response);
        }

        // ================= REFRESH TOKEN =================

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenRequestDto dto)
        {
            var user = _userManager.Users.FirstOrDefault(u => u.RefreshToken == dto.RefreshToken);

            if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                return Unauthorized(new AuthResponseDto { IsSuccess = false, Message = "Invalid refresh token" });

            var response = await CreateTokenResponse(user);
            return Ok(response);
        }

        // ================= FORGOT PASSWORD (FROM TOKEN) =================

        [Authorize]
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (email == null) return Unauthorized();

            await _otpService.GenerateAndSendAsync(email, OtpType.ResetPassword);

            return Ok(new AuthResponseDto
            {
                IsSuccess = true,
                Message = "Password reset code sent to your email."
            });
        }

        // ================= RESET PASSWORD (FROM TOKEN) =================

        [Authorize]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto request)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (email == null) return Unauthorized();

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return Unauthorized();

            var isValid = await _otpService.ValidateAsync(email, request.Code);
            if (!isValid) return BadRequest(new AuthResponseDto { IsSuccess = false, Message = "Invalid code" });

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, resetToken, request.NewPassword);

            if (!result.Succeeded)
                return BadRequest(new AuthResponseDto { IsSuccess = false, Message = "Password reset failed" });

            return Ok(new AuthResponseDto { IsSuccess = true, Message = "Password reset successful." });
        }

        // ================= HELPERS =================

        private async Task<AuthResponseDto> CreateTokenResponse(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);

            var accessToken = GenerateJwtToken(user, roles);
            var refreshToken = GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            await _userManager.UpdateAsync(user);

            return new AuthResponseDto
            {
                IsSuccess = true,
                Token = accessToken,
                RefreshToken = refreshToken,
                Roles = roles.ToList()
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