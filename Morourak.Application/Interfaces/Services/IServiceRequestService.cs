using Morourak.Application.DTOs;
using Morourak.Domain.Entities;
using Morourak.Domain.Enums.Request;

namespace Morourak.Application.Interfaces.Services
{
    public interface IServiceRequestService
    {
        /// <summary>
        /// إنشاء طلب خدمة جديد وإرجاعه كـ DTO
        /// </summary>
        /// <param name="serviceType">نوع الخدمة</param>
        /// <param name="referenceId">الرقم المرجعي المرتبط بالخدمة</param>
        /// <param name="status">الحالة الأولية للطلب</param>
        /// <returns>ServiceRequestDto الجديد</returns>
        Task<ServiceRequest> CreateAsync(ServiceType serviceType, int referenceId, RequestStatus status, string citizenNationalId);
        /// <summary>
        /// جلب جميع الطلبات الخاصة بالمستخدم الحالي
        /// </summary>
        /// <returns>قائمة من ServiceRequestDto</returns>
        Task<IReadOnlyList<ServiceRequestDto>> GetCitizenRequestsAsync();

        /// <summary>
        /// جلب طلب خدمة حسب رقم الطلب
        /// </summary>
        /// <param name="requestNumber">رقم الطلب</param>
        /// <returns>ServiceRequestDto أو null إذا لم يوجد</returns>
        Task<ServiceRequestDto?> GetByRequestNumberAsync(string requestNumber);

        /// <summary>
        /// تحديث حالة الطلب وإرجاع الـ DTO بعد التحديث
        /// </summary>
        /// <param name="requestNumber">رقم الطلب</param>
        /// <param name="status">الحالة الجديدة</param>
        /// <returns>ServiceRequestDto بعد التحديث</returns>
        Task<ServiceRequestDto> UpdateStatusAsync(string requestNumber, RequestStatus status);

        /// <summary>
        /// يُحدث حالة الدفع للطلب ويغير حالته إلى "جاهز للمعالجة" إذا تم الدفع بنجاح
        /// </summary>
        Task<ServiceRequestDto> MarkAsPaidAsync(string requestNumber, string transactionId, decimal amount);

        /// <summary>
        /// يُحدد طريقة التوصيل ويحسب الرسوم الإجمالية للطلب
        /// </summary>
        Task<ServiceRequestDto> SetDeliveryAndFeesAsync(string requestNumber, Morourak.Domain.Enums.Common.DeliveryMethod method, string? address);
    }
}