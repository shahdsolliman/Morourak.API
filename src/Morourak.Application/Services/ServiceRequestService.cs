using Morourak.Application.Common;
using Morourak.Application.DTOs;
using Morourak.Application.Interfaces;
using Morourak.Application.Interfaces.Services;
using Morourak.Domain.Entities;
using Morourak.Domain.Enums.Request;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Morourak.Domain.Extensions;
using AppEx = Morourak.Application.Exceptions;

namespace Morourak.Application.Services;

public class ServiceRequestService : IServiceRequestService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRequestNumberGenerator _generator;
    private readonly ICurrentUserService _currentUser;

    public ServiceRequestService(
        IUnitOfWork unitOfWork,
        IRequestNumberGenerator generator,
        ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _generator = generator;
        _currentUser = currentUser;
    }

    private ServiceRequestDto ToDto(ServiceRequest request)
    {
        return new ServiceRequestDto
        {
            RequestNumber = request.RequestNumber,
            CitizenNationalId = request.CitizenNationalId,
            ServiceType = request.ServiceType.GetDisplayName(),
            Status = request.Status.GetDisplayName(),
            SubmittedAt = request.SubmittedAt,
            LastUpdatedAt = request.LastUpdatedAt,
            ReferenceId = request.ReferenceId,
            Fees = new ServiceRequestFeesDto
            {
                BaseFee = request.BaseFee,
                DeliveryFee = request.DeliveryFee,
                TotalAmount = request.TotalAmount
            },
            Delivery = new ServiceRequestDeliveryDto
            {
                Method = request.DeliveryMethod?.GetDisplayName(),
                Address = request.DeliveryAddressDetail
            },
            Payment = new ServiceRequestPaymentDto
            {
                Status = request.PaymentStatus.GetDisplayName(),
                TransactionId = request.PaymentTransactionId,
                Amount = request.PaymentAmount,
                Timestamp = request.PaymentTimestamp
            }
        };
    }
    public async Task<IReadOnlyList<ServiceRequestDto>> GetCitizenRequestsAsync()
    {
        var nationalId = _currentUser.NationalId
            ?? throw new AppEx.ValidationException("رقم الهوية غير موجود في رمز التحقق.", "AUTH_MISSING_NATIONAL_ID");

        var requests = await _unitOfWork.Repository<ServiceRequest>()
            .FindAsync(x => x.CitizenNationalId == nationalId);

        return requests.Select(ToDto).ToList();
    }

    public async Task<ServiceRequestDto?> GetByRequestNumberAsync(string requestNumber)
    {
        var request = await _unitOfWork.Repository<ServiceRequest>()
            .GetAsync(x => x.RequestNumber == requestNumber);

        return request == null ? null : ToDto(request);
    }

    public async Task<ServiceRequestDto> UpdateStatusAsync(string requestNumber, RequestStatus status)
    {
        var repo = _unitOfWork.Repository<ServiceRequest>();

        var request = await repo.GetAsync(x => x.RequestNumber == requestNumber)
            ?? throw new AppEx.ValidationException("طلب الخدمة غير موجود.", "REQUEST_NOT_FOUND");

        request.Status = status;
        request.LastUpdatedAt = DateTime.UtcNow;

        repo.Update(request);
        await _unitOfWork.CommitAsync();

        return ToDto(request);
    }

    public async Task<ServiceRequest> CreateAsync(
    ServiceType serviceType,
    int referenceId,
    RequestStatus status,
    string citizenNationalId)
    {
        if (string.IsNullOrWhiteSpace(citizenNationalId))
            citizenNationalId = _currentUser.NationalId
                ?? throw new AppEx.ValidationException("رقم الهوية مطلوب.", "AUTH_MISSING_NATIONAL_ID");

        var request = new ServiceRequest
        {
            RequestNumber = await _generator.GenerateAsync(serviceType), 
            CitizenNationalId = citizenNationalId,
            ServiceType = serviceType,
            Status = status,
            ReferenceId = referenceId,
            PaymentStatus = PaymentStatus.Pending,
            SubmittedAt = DateTime.UtcNow,
            LastUpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<ServiceRequest>().AddAsync(request);
        await _unitOfWork.CommitAsync();

        return request;
    }

    // This method is now primarily used by PaymentService.FinalizePaymentAsync
    public async Task<ServiceRequestDto> MarkAsPaidAsync(string requestNumber, string transactionId, decimal amount)
    {
        var repo = _unitOfWork.Repository<ServiceRequest>();

        var request = await repo.GetAsync(x => x.RequestNumber == requestNumber)
            ?? throw new AppEx.ValidationException("طلب الخدمة غير موجود.", "REQUEST_NOT_FOUND");

        if (request.PaymentStatus == PaymentStatus.Paid)
            return ToDto(request);

        request.PaymentStatus = PaymentStatus.Paid;
        request.PaymentTransactionId = transactionId;
        request.PaymentAmount = amount;
        request.PaymentTimestamp = DateTime.UtcNow;
        request.Status = RequestStatus.ReadyForProcessing;
        request.LastUpdatedAt = DateTime.UtcNow;

        repo.Update(request);
        await _unitOfWork.CommitAsync();

        return ToDto(request);
    }

    public async Task<ServiceRequestDto> SetDeliveryAndFeesAsync(string requestNumber, Morourak.Domain.Enums.Common.DeliveryMethod method, string? address)
    {
        var repo = _unitOfWork.Repository<ServiceRequest>();
        var request = await repo.GetAsync(x => x.RequestNumber == requestNumber)
            ?? throw new AppEx.ValidationException("طلب الخدمة غير موجود.", "REQUEST_NOT_FOUND");

        // Set Base Fee based on service type
        request.BaseFee = request.ServiceType switch
        {
            ServiceType.DrivingLicenseIssue or ServiceType.DrivingLicenseRenewal or 
            ServiceType.DrivingLicenseReplacementLost or ServiceType.DrivingLicenseReplacementDamaged => FeeConstants.LicenseIssuanceFee,
            // Add other service types as needed
            _ => FeeConstants.LicenseIssuanceFee 
        };

        request.DeliveryMethod = method;
        request.DeliveryAddressDetail = address;
        request.DeliveryFee = (method == Morourak.Domain.Enums.Common.DeliveryMethod.HomeDelivery) ? FeeConstants.DeliveryFee : 0;
        request.TotalAmount = request.BaseFee + request.DeliveryFee;
        
        // Transition to AwaitingPayment only if currently Pending
        if (request.Status == RequestStatus.Pending)
        {
            request.Status = RequestStatus.AwaitingPayment;
        }
        request.LastUpdatedAt = DateTime.UtcNow;

        repo.Update(request);
        await _unitOfWork.CommitAsync();

        return ToDto(request);
    }
}
