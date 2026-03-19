using System.ComponentModel.DataAnnotations;

namespace Morourak.Application.DTOs.Paymob;

public class DirectPaymentDto
{
    [Required]
    public string RequestNumber { get; set; } = string.Empty;

    public List<int>? ViolationIds { get; set; }

    [Required]
    [CreditCard]
    public string CardNumber { get; set; } = string.Empty;

    [Required]
    public string CardholderName { get; set; } = string.Empty;

    [Required]
    [Range(1, 12)]
    public int ExpiryMonth { get; set; }

    [Required]
    [ValidExpiryYear]
    public int ExpiryYear { get; set; }

    [Required]
    [StringLength(4, MinimumLength = 3)]
    public string Cvv { get; set; } = string.Empty;
}

/// <summary>
/// Validates that the expiry year (2-digit) is not in the past. Dynamically computed.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class ValidExpiryYearAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is int year)
        {
            int currentYearShort = DateTime.UtcNow.Year % 100;
            if (year < currentYearShort)
                return new ValidationResult($"سنة انتهاء الصلاحية يجب ألا تكون أقل من {currentYearShort}.");
        }
        return ValidationResult.Success;
    }
}
