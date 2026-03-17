using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
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

namespace Morourak.Infrastructure.Services;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly IOtpService _otpService;
    private readonly ICitizenRegistryService _citizenRegistryService;

    public IdentityService(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        IOtpService otpService,
        ICitizenRegistryService citizenRegistryService)
    {
        _userManager = userManager;
        _configuration = configuration;
        _otpService = otpService;
        _citizenRegistryService = citizenRegistryService;
    }

    public async Task<AuthResponseDto> LoginAsync(string mobileNumber, string password)
    {
        var normalizedPhone = NormalizePhoneNumber(mobileNumber);
        var user = _userManager.Users.FirstOrDefault(u => u.PhoneNumber == normalizedPhone);

        if (user == null || user.IsDeleted)
            return new AuthResponseDto { IsSuccess = false, Message = "بيانات الدخول غير صحيحة.", ErrorCode = "INVALID_CREDENTIALS" };

        if (!await _userManager.CheckPasswordAsync(user, password))
            return new AuthResponseDto { IsSuccess = false, Message = "بيانات الدخول غير صحيحة.", ErrorCode = "INVALID_CREDENTIALS" };

        if (!user.IsVerified)
            return new AuthResponseDto { IsSuccess = false, Message = "الحساب غير مفعل. يرجى تفعيل الحساب أولاً.", ErrorCode = "UNVERIFIED_ACCOUNT" };

        return await CreateTokenResponseAsync(user.Id);
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        var matchResult = await _citizenRegistryService.ValidateFullMatchAsync(
            request.NationalId,
            request.FirstName,
            request.LastName,
            request.MobileNumber);

        if (!matchResult.IsMatch)
            return new AuthResponseDto { IsSuccess = false, Message = matchResult.Message, ErrorCode = "REGISTRY_MISMATCH" };

        if (await _userManager.FindByEmailAsync(request.Email) != null)
            return new AuthResponseDto { IsSuccess = false, Message = "البريد الإلكتروني مسجل بالفعل.", ErrorCode = "EMAIL_EXISTS" };

        if (_userManager.Users.Any(u => u.NationalId == request.NationalId))
            return new AuthResponseDto { IsSuccess = false, Message = "يوجد حساب مسجل بالفعل لهذا الرقم القومي.", ErrorCode = "NATIONAL_ID_EXISTS" };

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
            return new AuthResponseDto { IsSuccess = false, Message = "فشل في إنشاء حساب المستخدم.", ErrorCode = "CREATE_FAILED" };

        await _userManager.AddToRoleAsync(user, AppIdentityConstants.Roles.Citizen);
        await _otpService.GenerateAndSendAsync(user.Email!, OtpType.Register);

        return new AuthResponseDto { IsSuccess = true, Message = "تم إرسال رمز التحقق إلى بريدك الإلكتروني." };
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken)
    {
        var user = _userManager.Users.FirstOrDefault(u => u.RefreshToken == refreshToken);

        if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            return new AuthResponseDto { IsSuccess = false, Message = "رمز التحديث غير صالح.", ErrorCode = "INVALID_REFRESH_TOKEN" };

        return await CreateTokenResponseAsync(user.Id);
    }

    public async Task<AuthResponseDto> CreateTokenResponseAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return new AuthResponseDto { IsSuccess = false, Message = "المستخدم غير موجود." };

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

        var keyString = _configuration["Jwt:Key"];
        if (string.IsNullOrEmpty(keyString)) throw new InvalidOperationException("JWT Key not found in configuration.");
        
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expireMinutes = Convert.ToDouble(_configuration["Jwt:DurationInMinutes"] ?? "60");
        var expires = DateTime.UtcNow.AddMinutes(expireMinutes);

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
