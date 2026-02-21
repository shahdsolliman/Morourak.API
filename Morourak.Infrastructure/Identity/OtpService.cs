using Microsoft.AspNetCore.Identity;
using Morourak.Application.Interfaces.Services;
using System.Security.Cryptography;

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
                ?? throw new InvalidOperationException("User not found.");

            // Prevent OTP spam
            if (user.VerificationCodeExpiry.HasValue &&
                user.VerificationCodeExpiry > DateTime.UtcNow.AddMinutes(-ResendCooldownMinutes))
            {
                throw new InvalidOperationException(
                    "Please wait before requesting another verification code.");
            }

            var code = GenerateSecureOtp();

            user.VerificationCode = code;
            user.VerificationCodeExpiry = DateTime.UtcNow.AddMinutes(OtpExpiryMinutes);
            await _userManager.UpdateAsync(user);

            var subject = type == OtpType.Register
                ? "Morourak Registration Verification Code"
                : "Morourak Password Reset Code";

            var body = $"""
                <h2>Your verification code is: {code}</h2>
                <p>Valid for {OtpExpiryMinutes} minutes.</p>
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

            if (user.VerificationCode != code ||
                user.VerificationCodeExpiry < DateTime.UtcNow)
                return false;

            user.VerificationCode = null;
            user.VerificationCodeExpiry = null;
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