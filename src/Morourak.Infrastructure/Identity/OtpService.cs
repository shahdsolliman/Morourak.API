using Microsoft.AspNetCore.Identity;
using Morourak.Application.Interfaces.Services;
using Morourak.Infrastructure.Persistence;
using Morourak.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.ComponentModel.DataAnnotations;
using AppEx = Morourak.Application.Exceptions;

namespace Morourak.Infrastructure.Identity
{
    public class OtpService : IOtpService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly PersistenceDbContext _persistenceContext;
        private readonly IMailService _mailService;

        private const int OtpExpiryMinutes = 10;
        private const int ResendCooldownMinutes = 1;

        public OtpService(
            UserManager<ApplicationUser> userManager,
            PersistenceDbContext persistenceContext,
            IMailService mailService)
        {
            _userManager = userManager;
            _persistenceContext = persistenceContext;
            _mailService = mailService;
        }

        public async Task<string> GenerateAndSendAsync(
            string identifier,
            OtpType type = OtpType.Register)
        {
            string code = GenerateSecureOtp();
            DateTime expiry = DateTime.UtcNow.AddMinutes(OtpExpiryMinutes);

            if (type == OtpType.Register)
            {
                // identifier is Email for registration flow as per user correction
                var pending = await _persistenceContext.PendingRegistrations
                    .FirstOrDefaultAsync(p => p.Email == identifier);

                if (pending == null)
                    throw new AppEx.ValidationException("لم يتم العثور على طلب تسجيل لهذا البريد الإلكتروني.", "REGISTRATION_NOT_FOUND");

                if (pending.OtpExpiry.HasValue &&
                    pending.OtpExpiry > DateTime.UtcNow.AddMinutes(OtpExpiryMinutes - ResendCooldownMinutes))
                {
                    throw new AppEx.ValidationException("الرجاء الانتظار قبل طلب رمز تحقق جديد.", "OTP_COOLDOWN");
                }

                pending.OtpCode = code;
                pending.OtpExpiry = expiry;
                pending.OtpAttempts = 0;
                await _persistenceContext.SaveChangesAsync();

                await SendOtpAsync(pending.Email, code, type);
            }
            else
            {
                // Generic OtpVerification (e.g. ResetPassword)
                var otp = await _persistenceContext.OtpVerifications
                    .FirstOrDefaultAsync(o => o.Identifier == identifier && o.Type == type.ToString());

                if (otp == null)
                {
                    otp = new OtpVerification
                    {
                        Identifier = identifier,
                        Code = code,
                        Expiry = expiry,
                        Type = type.ToString()
                    };
                    _persistenceContext.OtpVerifications.Add(otp);
                }
                else
                {
                    if (otp.Expiry > DateTime.UtcNow.AddMinutes(OtpExpiryMinutes - ResendCooldownMinutes))
                    {
                        throw new AppEx.ValidationException("الرجاء الانتظار قبل طلب رمز تحقق جديد.", "OTP_COOLDOWN");
                    }

                    otp.Code = code;
                    otp.Expiry = expiry;
                    otp.Attempts = 0;
                    _persistenceContext.OtpVerifications.Update(otp);
                }

                await _persistenceContext.SaveChangesAsync();

                if (IsEmail(identifier))
                {
                    await SendOtpAsync(identifier, code, type);
                }
                else
                {
                    // Get user email to send code
                    var user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == identifier);
                    if (user != null)
                    {
                        await SendOtpAsync(user.Email!, code, type);
                    }
                }
            }

            return code;
        }

        public async Task<bool> ValidateAsync(string identifier, string code)
        {
            // Try Pending Registration first (Email based)
            var pending = await _persistenceContext.PendingRegistrations
                .FirstOrDefaultAsync(p => p.Email == identifier);

            if (pending != null)
            {
                if (pending.OtpCode == null || pending.OtpExpiry < DateTime.UtcNow)
                    return false;

                if (pending.OtpAttempts >= 5)
                {
                    pending.OtpCode = null;
                    pending.OtpExpiry = null;
                    await _persistenceContext.SaveChangesAsync();
                    return false;
                }

                if (pending.OtpCode != code)
                {
                    pending.OtpAttempts++;
                    await _persistenceContext.SaveChangesAsync();
                    return false;
                }

                return true;
            }

            // Try generic OtpVerification
            var otp = await _persistenceContext.OtpVerifications
                .FirstOrDefaultAsync(o => o.Identifier == identifier);

            if (otp != null)
            {
                if (otp.Expiry < DateTime.UtcNow)
                    return false;

                if (otp.Attempts >= 5)
                {
                    _persistenceContext.OtpVerifications.Remove(otp);
                    await _persistenceContext.SaveChangesAsync();
                    return false;
                }

                if (otp.Code != code)
                {
                    otp.Attempts++;
                    await _persistenceContext.SaveChangesAsync();
                    return false;
                }

                // Delete on success
                _persistenceContext.OtpVerifications.Remove(otp);
                await _persistenceContext.SaveChangesAsync();
                return true;
            }

            return false;
        }

        private async Task SendOtpAsync(string email, string code, OtpType type)
        {
            var subject = type == OtpType.Register
                ? "Morourak Registration Verification Code"
                : type == OtpType.ResetPassword
                    ? "Morourak Password Reset Code"
                    : "Morourak Change Email Verification Code";

            var body = $"""
<div style="font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #F4F6F8; padding: 50px 0; text-align: center;">
    <div style="display: inline-block; background-color: #ffffff; padding: 40px 50px; border-radius: 12px; box-shadow: 0 8px 24px rgba(0,0,0,0.12); max-width: 480px; text-align: center; direction: rtl;">
        <h1 style="color: #27AE60; font-size: 28px; margin-bottom: 25px;">رمز التحقق الخاص بك</h1>
        <div style="background-color: #E8F5E9; border: 1px solid #27AE60; border-radius: 10px; padding: 25px; font-size: 36px; font-weight: bold; color: #27AE60; display: inline-block; margin-bottom: 25px; direction: ltr;">
            {code}
        </div>
        <p style="font-size: 16px; color: #333; margin-bottom: 30px; direction: rtl; text-align: center;">
            صالح لمدة {OtpExpiryMinutes} دقائق. يرجى عدم مشاركة الرمز مع أي شخص.
        </p>
        <hr style="border: 0; border-top: 1px solid #27AE60; margin: 30px 0;">
        <p style="font-size: 14px; color: #555; direction: rtl; text-align: center;">
            إذا لم تطلب هذا الرمز، يمكنك تجاهل هذا البريد بأمان.
        </p>
    </div>
</div>
""";
            await _mailService.SendAsync(email, subject, body);
        }

        private static string GenerateSecureOtp()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[4];
            rng.GetBytes(bytes);
            var number = BitConverter.ToUInt32(bytes, 0) % 900000 + 100000;
            return number.ToString();
        }

        private static bool IsEmail(string identifier)
            => new EmailAddressAttribute().IsValid(identifier);
    }
}
