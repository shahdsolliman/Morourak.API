using Morourak.Application.DTOs.Paymob;
using Morourak.Domain.Enums.Request;

namespace Morourak.Application.Interfaces.Services;

public interface IPaymentService
{
    Task<PaymobPaymentResponse> CreatePaymentAsync(PaymentCreateRequest dto);
    Task<PaymentReceiptDto> GetReceiptAsync(string merchantOrderId);
    Task<bool> FinalizePaymentAsync(string paymobOrderId, string transactionId, bool success, string? merchantOrderId = null);
    Task<decimal> CalculateFeesAsync(string? requestNumber, List<int>? violationIds);
    Task<PaymentStatus> GetStatusAsync(string merchantOrderId);
}
