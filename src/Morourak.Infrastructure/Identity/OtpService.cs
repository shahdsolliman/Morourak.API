using Microsoft.AspNetCore.Identity;
using Morourak.Application.Interfaces.Services;
using System.Security.Cryptography;
using AppEx = Morourak.Application.Exceptions;

namespace Morourak.Infrastructure.Identity
{
    public class OtpService : IOtpService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMailService _mailService;

        private const int OtpExpiryMinutes = 10;
        private const int ResendCooldownMinutes = 1;

        public OtpService(
            UserManager<ApplicationUser> userManager,
            IMailService mailService)
        {
            _userManager = userManager;
            _mailService = mailService;
        }

        public async Task<string> GenerateAndSendAsync(
            string email,
            OtpType type = OtpType.Register)
        {
            var user = await _userManager.FindByEmailAsync(email)
                ?? throw new AppEx.ValidationException("المستخدم غير موجود.", "USER_NOT_FOUND");

            // Prevent OTP spam
            if (user.VerificationCodeExpiry.HasValue &&
                user.VerificationCodeExpiry > DateTime.UtcNow.AddMinutes(-ResendCooldownMinutes))
            {
                throw new AppEx.ValidationException(
                    "الرجاء الانتظار قبل طلب رمز تحقق جديد.", "OTP_COOLDOWN");
            }

            var code = GenerateSecureOtp();

            user.VerificationCode = code;
            user.VerificationCodeExpiry = DateTime.UtcNow.AddMinutes(OtpExpiryMinutes);
            user.OtpAttempts = 0; // Reset attempts
            await _userManager.UpdateAsync(user);

            var subject = type == OtpType.Register
                ? "Morourak Registration Verification Code"
                : "Morourak Password Reset Code";

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

        <p style="font-size: 14px; color: #777; margin-top: 15px; direction: ltr; text-align: center;">
            © 2026 Morourak. All rights reserved.
        </p>
    </div>
</div>
""";

            try
            {
                await _mailService.SendAsync(email, subject, body);
            }
            catch
            {
                // rollback OTP if email fails
                user.VerificationCode = null;
                user.VerificationCodeExpiry = null;
                await _userManager.UpdateAsync(user);
                throw;
            }

            return code;
        }

        public async Task<bool> ValidateAsync(string email, string code)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return false;

            if (user.VerificationCode == null || user.VerificationCodeExpiry < DateTime.UtcNow)
                return false;

            if (user.OtpAttempts >= 5) // Brute force protection
            {
                user.VerificationCode = null;
                user.VerificationCodeExpiry = null;
                await _userManager.UpdateAsync(user);
                return false;
            }

            if (user.VerificationCode != code)
            {
                user.OtpAttempts++;
                await _userManager.UpdateAsync(user);
                return false;
            }

            // Success
            user.VerificationCode = null;
            user.VerificationCodeExpiry = null;
            user.OtpAttempts = 0;
            await _userManager.UpdateAsync(user);

            return true;
        }

        private static string GenerateSecureOtp()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[4];
            rng.GetBytes(bytes);

            var number = BitConverter.ToUInt32(bytes, 0) % 900000 + 100000;
            return number.ToString();
        }
    }
}