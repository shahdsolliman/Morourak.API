using Morourak.Application.DTOs;
using Morourak.Domain.Entities;
using Morourak.Domain.Enums.Request;
using Morourak.Domain.Enums.Common;

namespace Morourak.Application.Interfaces.Services
{
    public interface IServiceRequestService
    {
        /// <summary>
        /// يُنشئ طلب خدمة جديد.
        /// </summary>
        Task<ServiceRequest> CreateAsync(ServiceType serviceType, int referenceId, RequestStatus status, string citizenNationalId);

        /// <summary>
        /// يُحدد طريقة التوصيل ويحسب الرسوم لإصدار/تجديد/بدل الرخصة.
        /// يستخدم الكيان الممرر مباشرة لتجنب مشاكل تعقب الهوية في EF Core.
        /// </summary>
        Task<ServiceRequestDto> SetDeliveryAndFeesAsync(ServiceRequest request, DeliveryMethod method, string? address);

        /// <summary>
        /// يُحدد طريقة التوصيل ويحسب الرسوم باستخدام رقم الطلب (للاستدعاءات المنفصلة).
        /// </summary>
        Task<ServiceRequestDto> SetDeliveryAndFeesAsync(string requestNumber, DeliveryMethod method, string? address);

        Task<IReadOnlyList<ServiceRequestDto>> GetCitizenRequestsAsync();
        Task<ServiceRequestDto?> GetByRequestNumberAsync(string requestNumber);
        Task<ServiceRequestDto> UpdateStatusAsync(string requestNumber, RequestStatus status);
        Task<ServiceRequestDto> MarkAsPaidAsync(string requestNumber, string transactionId, decimal amount);
    }
}