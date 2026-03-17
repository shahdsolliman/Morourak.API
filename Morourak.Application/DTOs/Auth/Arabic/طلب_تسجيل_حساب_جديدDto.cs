using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Morourak.Application.DTOs.Auth.Arabic
{
    /// <summary>
    /// Request DTO used for citizen registration.
    /// Registration is allowed only if NationalId and MobileNumber
    /// match an existing record in CitizenRegistry.
    /// </summary>
    public class طلب_تسجيل_حساب_جديدDto
    {
        /// <summary>
        /// The 14-digit national identity number of the citizen.
        /// </summary>
        /// <example>29001011234567</example>
        [Required]
        [StringLength(14, MinimumLength = 14, ErrorMessage = "الرقم القومي يجب أن يكون 14 رقماً بالضبط.")]
        [JsonPropertyName("الرقم القومي")]
        public string الرقم_القومي { get; set; } = null!;

        /// <summary>
        /// The mobile phone number of the citizen.
        /// </summary>
        /// <example>01234567890</example>
        [Required]
        [Phone]
        [JsonPropertyName("رقم الموبايل")]
        public string رقم_الموبايل { get; set; } = null!;

        /// <summary>
        /// The chosen username for the account.
        /// </summary>
        /// <example>johndoe</example>
        [Required]
        [MinLength(4)]
        [JsonPropertyName("اسم المستخدم")]
        public string اسم_المستخدم { get; set; } = null!;

        /// <summary>
        /// The email address for account notifications and verification.
        /// </summary>
        /// <example>john.doe@example.com</example>
        [Required]
        [EmailAddress]
        [JsonPropertyName("البريد الإلكتروني")]
        public string البريد_الإلكتروني { get; set; } = null!;

        /// <summary>
        /// The citizen's first name as it appears on their ID.
        /// </summary>
        /// <example>John</example>
        [Required]
        [JsonPropertyName("الاسم الأول")]
        public string الاسم_الأول { get; set; } = null!;

        /// <summary>
        /// The citizen's last name as it appears on their ID.
        /// </summary>
        /// <example>Doe</example>
        [Required]
        [JsonPropertyName("الاسم الأخير")]
        public string الاسم_الأخير { get; set; } = null!;

        /// <summary>
        /// The password for the new account.
        /// </summary>
        /// <example>P@ssword123</example>
        [Required]
        [DataType(DataType.Password)]
        [JsonPropertyName("كلمة المرور")]
        public string كلمة_المرور { get; set; } = null!;

        /// <summary>
        /// Confirmation of the password to ensure it was entered correctly.
        /// </summary>
        /// <example>P@ssword123</example>
        [Required]
        [Compare(nameof(كلمة_المرور), ErrorMessage = "كلمات المرور غير متطابقة.")]
        [JsonPropertyName("تأكيد كلمة المرور")]
        public string تأكيد_كلمة_المرور { get; set; } = null!;
    }
}
