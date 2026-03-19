using System.ComponentModel.DataAnnotations;

namespace Morourak.Application.DTOs.Paymob;

/// <summary>
/// Request to initiate a payment. At least one of <see cref="ServiceRequestNumber"/>
/// or <see cref="ViolationIds"/> must be provided.
/// </summary>
[AtLeastOnePaymentTarget]
public class PaymentCreateRequest
{
    /// <summary>The service request number to pay for (optional if ViolationIds provided).</summary>
    public string? ServiceRequestNumber { get; set; }

    /// <summary>List of traffic violation IDs to pay for (optional if ServiceRequestNumber provided).</summary>
    public List<int>? ViolationIds { get; set; }
}

/// <summary>
/// Validates that at least one payment target (service request or violations) is supplied.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class AtLeastOnePaymentTargetAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is PaymentCreateRequest req)
        {
            bool hasRequest = !string.IsNullOrWhiteSpace(req.ServiceRequestNumber);
            bool hasViolations = req.ViolationIds != null && req.ViolationIds.Count > 0;

            if (!hasRequest && !hasViolations)
                return new ValidationResult(
                    "يجب تحديد رقم طلب الخدمة أو معرّفات المخالفات على الأقل.",
                    new[] { nameof(req.ServiceRequestNumber), nameof(req.ViolationIds) });
        }
        return ValidationResult.Success;
    }
}
