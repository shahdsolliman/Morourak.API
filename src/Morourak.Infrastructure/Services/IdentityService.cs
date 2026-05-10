using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Morourak.Application.DTOs.Auth;
using Morourak.Application.Interfaces.Services;
using Morourak.Infrastructure.Identity;
using Morourak.Infrastructure.Identity.Constants;
using Morourak.Infrastructure.Persistence;
using Morourak.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Morourak.Infrastructure.Services;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly IOtpService _otpService;
    private readonly ICitizenRegistryService _citizenRegistryService;
    private readonly PersistenceDbContext _persistenceContext;
    private readonly IPasswordHasher<ApplicationUser> _passwordHasher;

    public IdentityService(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        IOtpService otpService,
        ICitizenRegistryService citizenRegistryService,
        PersistenceDbContext persistenceContext,
        IPasswordHasher<ApplicationUser> passwordHasher)
    {
        _userManager = userManager;
        _configuration = configuration;
        _otpService = otpService;
        _citizenRegistryService = citizenRegistryService;
        _persistenceContext = persistenceContext;
        _passwordHasher = passwordHasher;
    }

    public async Task<AuthResponseDto> LoginAsync(string mobileNumber, string password)
    {
        var normalizedPhone = NormalizePhoneNumber(mobileNumber);
        var user = _userManager.Users.FirstOrDefault(u => u.PhoneNumber == normalizedPhone);

        if (user == null)
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

        var normalizedPhone = NormalizePhoneNumber(request.MobileNumber);

        // Check if user already exists in AspNetUsers
        if (await _userManager.Users.IgnoreQueryFilters().AnyAsync(u => u.Email == request.Email))
            return new AuthResponseDto { IsSuccess = false, Message = "البريد الإلكتروني مسجل بالفعل.", ErrorCode = "EMAIL_EXISTS" };

        if (await _userManager.Users.IgnoreQueryFilters().AnyAsync(u => u.NationalId == request.NationalId))
            return new AuthResponseDto { IsSuccess = false, Message = "يوجد حساب مسجل بالفعل لهذا الرقم القومي.", ErrorCode = "NATIONAL_ID_EXISTS" };

        if (await _userManager.Users.IgnoreQueryFilters().AnyAsync(u => u.PhoneNumber == normalizedPhone))
            return new AuthResponseDto { IsSuccess = false, Message = "رقم الهاتف مسجل بالفعل.", ErrorCode = "PHONE_EXISTS" };
            
        if (await _userManager.Users.IgnoreQueryFilters().AnyAsync(u => u.UserName == request.Username))
            return new AuthResponseDto { IsSuccess = false, Message = "اسم المستخدم مسجل بالفعل.", ErrorCode = "USERNAME_EXISTS" };

        // Create or update PendingRegistration
        var pending = await _persistenceContext.PendingRegistrations
            .FirstOrDefaultAsync(p => p.PhoneNumber == normalizedPhone || p.Email == request.Email || p.NationalId == request.NationalId);

        if (pending == null)
        {
            pending = new PendingRegistration
            {
                PhoneNumber = normalizedPhone,
                Email = request.Email,
                Username = request.Username,
                FirstName = request.FirstName,
                LastName = request.LastName,
                NationalId = request.NationalId,
                PasswordHash = _passwordHasher.HashPassword(null!, request.Password)
            };
            _persistenceContext.PendingRegistrations.Add(pending);
        }
        else
        {
            // Update existing pending record
            pending.PhoneNumber = normalizedPhone;
            pending.Email = request.Email;
            pending.Username = request.Username;
            pending.FirstName = request.FirstName;
            pending.LastName = request.LastName;
            pending.NationalId = request.NationalId;
            pending.PasswordHash = _passwordHasher.HashPassword(null!, request.Password);
            pending.OtpAttempts = 0; // Reset attempts
            _persistenceContext.PendingRegistrations.Update(pending);
        }

        await _persistenceContext.SaveChangesAsync();

        // Send OTP to Email
        await _otpService.GenerateAndSendAsync(request.Email, OtpType.Register);

        return new AuthResponseDto 
        { 
            IsSuccess = true, 
            Message = "تم إرسال رمز التحقق إلى بريدك الإلكتروني." 
        };
    }

    public async Task<AuthResponseDto> ConfirmRegistrationAsync(string email, string code)
    {
        var pending = await _persistenceContext.PendingRegistrations
            .FirstOrDefaultAsync(p => p.Email == email);

        if (pending == null)
            return new AuthResponseDto { IsSuccess = false, Message = "لم يتم العثور على طلب تسجيل لهذا البريد الإلكتروني.", ErrorCode = "REGISTRATION_NOT_FOUND" };

        var isValid = await _otpService.ValidateAsync(email, code);
        if (!isValid)
            return new AuthResponseDto { IsSuccess = false, Message = "رمز التحقق غير صحيح أو منتهي الصلاحية.", ErrorCode = "INVALID_OTP" };

        // Create ApplicationUser
        var user = new ApplicationUser
        {
            UserName = pending.Username,
            Email = pending.Email,
            PhoneNumber = pending.PhoneNumber,
            FirstName = pending.FirstName,
            LastName = pending.LastName,
            NationalId = pending.NationalId,
            IsVerified = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Note: PasswordHash is already hashed in pending
        user.PasswordHash = pending.PasswordHash;

        var result = await _userManager.CreateAsync(user);
        if (!result.Succeeded)
        {
            var error = result.Errors.FirstOrDefault()?.Description ?? "فشل في إنشاء حساب المستخدم.";
            return new AuthResponseDto { IsSuccess = false, Message = error, ErrorCode = "CREATE_FAILED" };
        }

        await _userManager.AddToRoleAsync(user, AppIdentityConstants.Roles.Citizen);

        // Remove PendingRegistration
        _persistenceContext.PendingRegistrations.Remove(pending);
        await _persistenceContext.SaveChangesAsync();

        return new AuthResponseDto 
        { 
            IsSuccess = true, 
            Message = "تم تفعيل الحساب بنجاح. يمكنك الآن تسجيل الدخول." 
        };
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
            Roles = roles.ToList(),
            Username = user.UserName,
            FullName = $"{user.FirstName} {user.LastName}".Trim()
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
        
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString)) { KeyId = "MorourakSecretKey" };
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

    public async Task<AuthResponseDto> RequestChangeEmailAsync(string userId, string newEmail)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return new AuthResponseDto { IsSuccess = false, Message = "المستخدم غير موجود.", ErrorCode = "USER_NOT_FOUND" };

        if (await _userManager.Users.AnyAsync(u => u.Email == newEmail))
            return new AuthResponseDto { IsSuccess = false, Message = "البريد الإلكتروني مسجل بالفعل لمستخدم آخر.", ErrorCode = "EMAIL_EXISTS" };

        // Send OTP to the NEW email
        await _otpService.GenerateAndSendAsync(newEmail, OtpType.ChangeEmail);

        return new AuthResponseDto
        {
            IsSuccess = true,
            Message = "تم إرسال رمز التحقق إلى بريدك الإلكتروني الجديد."
        };
    }

    public async Task<AuthResponseDto> ConfirmChangeEmailAsync(string userId, string newEmail, string code)
    {
        var isValid = await _otpService.ValidateAsync(newEmail, code);
        if (!isValid)
            return new AuthResponseDto { IsSuccess = false, Message = "رمز التحقق غير صحيح أو منتهي الصلاحية.", ErrorCode = "INVALID_OTP" };

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return new AuthResponseDto { IsSuccess = false, Message = "المستخدم غير موجود.", ErrorCode = "USER_NOT_FOUND" };

        var token = await _userManager.GenerateChangeEmailTokenAsync(user, newEmail);
        var result = await _userManager.ChangeEmailAsync(user, newEmail, token);

        if (!result.Succeeded)
        {
            var error = result.Errors.FirstOrDefault()?.Description ?? "فشل في تحديث البريد الإلكتروني.";
            return new AuthResponseDto { IsSuccess = false, Message = error, ErrorCode = "CHANGE_EMAIL_FAILED" };
        }

        return new AuthResponseDto
        {
            IsSuccess = true,
            Message = "تم تغيير البريد الإلكتروني بنجاح."
        };
    }

    public async Task<AuthResponseDto> ForgotPasswordAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        
        // Security: Don't reveal if user exists or not
        if (user != null)
        {
            await _otpService.GenerateAndSendAsync(email, OtpType.ResetPassword);
        }

        return new AuthResponseDto
        {
            IsSuccess = true,
            Message = "إذا كان هذا البريد مسجلاً لدينا، فقد أرسلنا رمز التحقق إليه."
        };
    }

    public async Task<AuthResponseDto> ResetPasswordAsync(ResetPasswordDto request)
    {
        var isValid = await _otpService.ValidateAsync(request.Email, request.Code);
        if (!isValid)
            return new AuthResponseDto { IsSuccess = false, Message = "رمز التحقق غير صحيح أو منتهي الصلاحية.", ErrorCode = "INVALID_OTP" };

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return new AuthResponseDto { IsSuccess = false, Message = "المستخدم غير موجود.", ErrorCode = "USER_NOT_FOUND" };

        // Generate reset token and reset password
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);

        if (!result.Succeeded)
        {
            var error = result.Errors.FirstOrDefault()?.Description ?? "فشل في إعادة تعيين كلمة المرور.";
            return new AuthResponseDto { IsSuccess = false, Message = error, ErrorCode = "RESET_PASSWORD_FAILED" };
        }

        return new AuthResponseDto
        {
            IsSuccess = true,
            Message = "تم إعادة تعيين كلمة المرور بنجاح."
        };
    }
}
