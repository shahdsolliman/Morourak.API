using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Morourak.Application.DTOs.Auth;
using Morourak.Application.Interfaces.Services;
using Morourak.Infrastructure.Identity;
using Morourak.Infrastructure.Identity.Seed;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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

        // =========================
        // REGISTER (CREATE USER + SEND OTP)
        // =========================
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new AuthResponseDto { IsSuccess = false, Message = "Invalid registration data." });

            // Validate citizen registry
            var validationResult = await _citizenRegistryService.ValidateAsync(request.NationalId, request.MobileNumber);
            if (!validationResult.IsValid)
                return BadRequest(new AuthResponseDto { IsSuccess = false, Message = validationResult.Message });

            // Check email uniqueness
            if (await _userManager.FindByEmailAsync(request.Email) != null)
                return BadRequest(new AuthResponseDto { IsSuccess = false, Message = "Email already exists." });

            // Check National ID uniqueness
            if (_userManager.Users.Any(u => u.NationalId == request.NationalId))
                return BadRequest(new AuthResponseDto { IsSuccess = false, Message = "An account already exists for this National ID." });

            // Create user directly with IsVerified = false
            var user = new ApplicationUser
            {
                UserName = request.Username,
                Email = request.Email,
                PhoneNumber = request.MobileNumber,
                FirstName = request.FirstName,
                LastName = request.LastName,
                NationalId = request.NationalId,
                IsVerified = false
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
                return BadRequest(new AuthResponseDto { IsSuccess = false, Message = string.Join(", ", result.Errors.Select(e => e.Description)) });

            await _userManager.AddToRoleAsync(user, IdentityRoles.Citizen);

            // Generate OTP and send email

            await _otpService.GenerateAndSendAsync(user.Email, OtpType.Register);


            return Ok(new AuthResponseDto { IsSuccess = true, Message = "Verification code sent to your email." });
        }

        // =========================
        // VERIFY OTP (ACTIVATE ACCOUNT)
        // =========================
        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return BadRequest(new AuthResponseDto { IsSuccess = false, Message = "User not found." });

            var isValid = await _otpService.ValidateAsync(dto.Email, dto.Code);

            if (!isValid)
                return BadRequest(new AuthResponseDto { IsSuccess = false, Message = "Invalid or expired verification code." });

            user.IsVerified = true;
            await _userManager.UpdateAsync(user);

            return Ok(new AuthResponseDto { IsSuccess = true, Message = "User verified successfully." });
        }


        // =========================
        // LOGIN
        // =========================
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new LoginResponseDto { IsSuccess = false, Message = "Invalid login data." });

            var user = _userManager.Users.FirstOrDefault(u => u.PhoneNumber == request.PhoneNumber);

            if (user == null || !user.IsVerified)
                return Unauthorized(new LoginResponseDto { IsSuccess = false, Message = "Invalid phone number or account not verified." });

            var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!passwordValid)
                return Unauthorized(new LoginResponseDto { IsSuccess = false, Message = "Invalid phone number or password." });

            var roles = await _userManager.GetRolesAsync(user);
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim("NationalId", user.NationalId)
            };

            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(double.Parse(_configuration["Jwt:DurationInMinutes"]!)),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return Ok(new LoginResponseDto
            {
                IsSuccess = true,
                Message = "Login successful.",
                Token = tokenHandler.WriteToken(token),
                ExpiresAt = tokenDescriptor.Expires
            });
        }

        // =========================
        // FORGOT PASSWORD (SEND OTP)
        // =========================
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return NotFound(new AuthResponseDto { IsSuccess = false, Message = "User not found." });

            // Generate OTP and send email

            await _otpService.GenerateAndSendAsync(user.Email, OtpType.ResetPassword);


            return Ok(new AuthResponseDto { IsSuccess = true, Message = "Password reset code sent to your email." });
        }

        // =========================
        // RESET PASSWORD (VERIFY OTP + CHANGE PASSWORD)
        // =========================
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            if (dto.NewPassword != dto.ConfirmPassword)
                return BadRequest(new AuthResponseDto { IsSuccess = false, Message = "Passwords do not match." });

            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return NotFound(new AuthResponseDto { IsSuccess = false, Message = "User not found." });

            var isValid = await _otpService.ValidateAsync(dto.Email, dto.Code);
            if (!isValid)
                return BadRequest(new AuthResponseDto { IsSuccess = false, Message = "Invalid or expired OTP." });

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);

            if (!result.Succeeded)
                return BadRequest(new AuthResponseDto { IsSuccess = false, Message = string.Join(", ", result.Errors.Select(e => e.Description)) });

            return Ok(new AuthResponseDto { IsSuccess = true, Message = "Password reset successfully." });
        }

    }
}
