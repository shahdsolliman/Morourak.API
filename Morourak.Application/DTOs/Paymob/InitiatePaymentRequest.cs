using System.ComponentModel.DataAnnotations;

namespace Morourak.Application.DTOs.Paymob;

public class InitiatePaymentRequest
{
    [Required]
    public string RequestNumber { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    [Required]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [Phone]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    public string Country { get; set; } = "Egypt";

    [Required]
    public string City { get; set; } = string.Empty;

    [Required]
    public string Street { get; set; } = string.Empty;

    [Required]
    public string Building { get; set; } = string.Empty;
}
