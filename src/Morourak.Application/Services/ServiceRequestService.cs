using Morourak.Application.Common;
using Morourak.Application.DTOs;
using Morourak.Application.Interfaces;
using Morourak.Application.Interfaces.Services;
using Morourak.Domain.Entities;
using Morourak.Domain.Enums.Request;
using Morourak.Domain.Enums.Common;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Morourak.Domain.Extensions;
using AutoMapper;
using AppEx = Morourak.Application.Exceptions;

namespace Morourak.Application.Services;

public class ServiceRequestService : IServiceRequestService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRequestNumberGenerator _generator;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;

    public ServiceRequestService(
        IUnitOfWork unitOfWork,
        IRequestNumberGenerator generator,
        ICurrentUserService currentUser,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _generator = generator;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    private ServiceRequestDto ToDto(ServiceRequest request)
    {
        return _mapper.Map<ServiceRequestDto>(request);
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
        // We do NOT commit here. We let the caller decide or commit after full processing.
        // But for internal consistency with old logic, some callers might expect it.
        // However, to keep it clean, we move commit to service boundary.
        
        return request;
    }

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

    public async Task<ServiceRequestDto> SetDeliveryAndFeesAsync(string requestNumber, DeliveryMethod method, string? address)
    {
        var request = await _unitOfWork.Repository<ServiceRequest>()
            .GetAsync(x => x.RequestNumber == requestNumber)
            ?? throw new AppEx.ValidationException("طلب الخدمة غير موجود.", "REQUEST_NOT_FOUND");

        return await SetDeliveryAndFeesAsync(request, method, address);
    }

    public async Task<ServiceRequestDto> SetDeliveryAndFeesAsync(ServiceRequest request, DeliveryMethod method, string? address)
    {
        // 1. Persistence Guard: The upgraded GenericRepository.Update will 
        // handle identity unification if this entity is already tracked.

        // 2. Set Base Fee based on service type
        request.BaseFee = request.ServiceType switch
        {
            ServiceType.DrivingLicenseIssue or ServiceType.DrivingLicenseRenewal or 
            ServiceType.DrivingLicenseReplacementLost or ServiceType.DrivingLicenseReplacementDamaged => FeeConstants.LicenseIssuanceFee,
            _ => FeeConstants.LicenseIssuanceFee 
        };

        request.DeliveryMethod = method;
        request.DeliveryAddressDetail = address;
        request.DeliveryFee = (method == DeliveryMethod.HomeDelivery) ? FeeConstants.DeliveryFee : 0;
        
        decimal insuranceFee = 0;
        if (request.ServiceType == ServiceType.VehicleLicenseIssue)
        {
            var application = await _unitOfWork.Repository<VehicleLicenseApplication>().GetByIdAsync(request.ReferenceId);
            if (application != null && application.InsuranceCompanyId.HasValue)
            {
                var company = await _unitOfWork.Repository<InsuranceCompany>().GetByIdAsync(application.InsuranceCompanyId.Value);
                if (company != null)
                {
                    insuranceFee = company.Fee;
                }
            }
        }

        request.TotalAmount = request.BaseFee + request.DeliveryFee + insuranceFee;
        
        if (request.Status == RequestStatus.Pending)
        {
            request.Status = RequestStatus.AwaitingPayment;
        }
        request.LastUpdatedAt = DateTime.UtcNow;
        
        // Explicitly mark as modified because GenericRepository uses AsNoTracking by default.
        _unitOfWork.Repository<ServiceRequest>().Update(request);
        
        await _unitOfWork.CommitAsync();

        return ToDto(request);
    }
}
